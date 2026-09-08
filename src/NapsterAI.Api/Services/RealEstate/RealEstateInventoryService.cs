using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// Search/filter/sort/paginate over Inventory (individual sellable/leasable units) plus CRUD.
/// This is the primary endpoint the EdgeMCP "searchInventory" tool (Phase 9) will call.
/// </summary>
public class RealEstateInventoryService : IRealEstateInventoryService
{
    private static readonly string[] ValidSortFields = ["price", "area", "listingDate"];

    private readonly RealEstateDbContext _db;
    private readonly ILogger<RealEstateInventoryService> _logger;

    public RealEstateInventoryService(RealEstateDbContext db, ILogger<RealEstateInventoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResultDto<InventoryDto>> ListAsync(
        int? projectId, int? unitTypeId, int? minBedrooms, int? maxBedrooms,
        decimal? minPrice, decimal? maxPrice, decimal? minAreaSqFt, decimal? maxAreaSqFt,
        string? status, string? viewType, string? furnishingStatus, string? search,
        string? sortBy, bool sortDescending,
        int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        if (sortBy is not null && !ValidSortFields.Contains(sortBy))
        {
            throw new InvalidRequestException($"sortBy must be one of: {string.Join(", ", ValidSortFields)}.");
        }

        var query = _db.Inventory.AsNoTracking().AsQueryable();

        if (projectId is not null) query = query.Where(i => i.ProjectId == projectId);
        if (unitTypeId is not null) query = query.Where(i => i.UnitTypeId == unitTypeId);
        if (minBedrooms is not null) query = query.Where(i => i.Bedrooms >= minBedrooms);
        if (maxBedrooms is not null) query = query.Where(i => i.Bedrooms <= maxBedrooms);
        if (minPrice is not null) query = query.Where(i => i.ListPrice >= minPrice);
        if (maxPrice is not null) query = query.Where(i => i.ListPrice <= maxPrice);
        if (minAreaSqFt is not null) query = query.Where(i => i.AreaSqFt >= minAreaSqFt);
        if (maxAreaSqFt is not null) query = query.Where(i => i.AreaSqFt <= maxAreaSqFt);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(i => i.Status == status);
        if (!string.IsNullOrWhiteSpace(viewType)) query = query.Where(i => i.ViewType == viewType);
        if (!string.IsNullOrWhiteSpace(furnishingStatus)) query = query.Where(i => i.FurnishingStatus == furnishingStatus);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(i => i.UnitNumber.Contains(search));

        var totalCount = await _db.Inventory.CountAsync(cancellationToken);
        var filteredCount = await query.CountAsync(cancellationToken);

        query = (sortBy, sortDescending) switch
        {
            ("price", true) => query.OrderByDescending(i => i.ListPrice),
            ("price", false) => query.OrderBy(i => i.ListPrice),
            ("area", true) => query.OrderByDescending(i => i.AreaSqFt),
            ("area", false) => query.OrderBy(i => i.AreaSqFt),
            ("listingDate", true) => query.OrderByDescending(i => i.ListingDate),
            ("listingDate", false) => query.OrderBy(i => i.ListingDate),
            _ => query.OrderBy(i => i.ProjectId).ThenBy(i => i.UnitNumber)
        };

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(i => MapInventory(i))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<InventoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            FilteredCount = filteredCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<InventoryDto> GetByIdAsync(int inventoryId, CancellationToken cancellationToken)
    {
        var inventory = await FindAsync(inventoryId, cancellationToken);
        return MapInventory(inventory);
    }

    public async Task<InventoryDto> CreateAsync(CreateInventoryRequestDto request, CancellationToken cancellationToken)
    {
        await ValidateWriteAsync(request, cancellationToken);

        if (await _db.Inventory.AnyAsync(i => i.ProjectId == request.ProjectId && i.UnitNumber == request.UnitNumber, cancellationToken))
        {
            throw new RealEstateConflictException($"Unit number '{request.UnitNumber}' already exists in project {request.ProjectId}.");
        }

        var inventory = new Inventory { ApprovalStatus = RealEstateApprovalStatuses.Draft };
        ApplyToEntity(request, inventory);

        _db.Inventory.Add(inventory);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created inventory {InventoryId} ({UnitNumber}) for project {ProjectId}", inventory.InventoryId, inventory.UnitNumber, inventory.ProjectId);

        return MapInventory(inventory);
    }

    public async Task<InventoryDto> UpdateAsync(int inventoryId, UpdateInventoryRequestDto request, CancellationToken cancellationToken)
    {
        await ValidateWriteAsync(request, cancellationToken);

        var inventory = await FindAsync(inventoryId, cancellationToken);

        if (!string.Equals(inventory.UnitNumber, request.UnitNumber, StringComparison.Ordinal)
            && await _db.Inventory.AnyAsync(i => i.ProjectId == request.ProjectId && i.UnitNumber == request.UnitNumber && i.InventoryId != inventoryId, cancellationToken))
        {
            throw new RealEstateConflictException($"Unit number '{request.UnitNumber}' already exists in project {request.ProjectId}.");
        }

        ApplyToEntity(request, inventory);
        inventory.SoldOrLeasedDate = request.SoldOrLeasedDate;
        inventory.BuyerTenantName = request.BuyerTenantName;
        inventory.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return MapInventory(inventory);
    }

    public async Task DeleteAsync(int inventoryId, CancellationToken cancellationToken)
    {
        var inventory = await FindAsync(inventoryId, cancellationToken);
        _db.Inventory.Remove(inventory);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Inventory> FindAsync(int inventoryId, CancellationToken cancellationToken)
    {
        var inventory = await _db.Inventory.FirstOrDefaultAsync(i => i.InventoryId == inventoryId, cancellationToken);
        if (inventory is null)
        {
            throw new RealEstateResourceNotFoundException($"Inventory unit {inventoryId} was not found.");
        }

        return inventory;
    }

    private async Task ValidateWriteAsync(CreateInventoryRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UnitNumber))
        {
            throw new InvalidRequestException("unitNumber is required.");
        }

        if (!RealEstateInventoryStatuses.All.Contains(request.Status))
        {
            throw new InvalidRequestException($"status must be one of: {string.Join(", ", RealEstateInventoryStatuses.All)}.");
        }

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new InvalidRequestException($"projectId {request.ProjectId} does not reference an existing project.");
        }

        var unitType = await _db.UnitTypes.FirstOrDefaultAsync(u => u.UnitTypeId == request.UnitTypeId, cancellationToken);
        if (unitType is null || unitType.ProjectId != request.ProjectId)
        {
            throw new InvalidRequestException($"unitTypeId {request.UnitTypeId} does not reference an existing unit type on project {request.ProjectId}.");
        }

        if (request.SalesAgentId is not null && !await _db.SalesAgents.AnyAsync(a => a.SalesAgentId == request.SalesAgentId, cancellationToken))
        {
            throw new InvalidRequestException($"salesAgentId {request.SalesAgentId} does not reference an existing sales agent.");
        }
    }

    private static void ApplyToEntity(CreateInventoryRequestDto request, Inventory inventory)
    {
        inventory.ProjectId = request.ProjectId;
        inventory.UnitTypeId = request.UnitTypeId;
        inventory.UnitNumber = request.UnitNumber;
        inventory.BuildingTower = request.BuildingTower;
        inventory.FloorNumber = request.FloorNumber;
        inventory.ViewType = request.ViewType;
        inventory.AreaSqFt = request.AreaSqFt;
        inventory.Bedrooms = request.Bedrooms;
        inventory.Bathrooms = request.Bathrooms;
        inventory.ParkingSpaces = request.ParkingSpaces;
        inventory.HasBalcony = request.HasBalcony;
        inventory.FurnishingStatus = request.FurnishingStatus;
        inventory.ListPrice = request.ListPrice;
        inventory.PricePerSqFt = request.PricePerSqFt;
        inventory.Status = request.Status;
        inventory.ListingDate = request.ListingDate;
        inventory.SalesAgentId = request.SalesAgentId;
        inventory.AgentName = request.AgentName;
        inventory.AgentContact = request.AgentContact;
        inventory.Notes = request.Notes;
    }

    private static void ValidatePaging(int pageIndex, int pageSize, int maxPageSize)
    {
        if (pageIndex < 0)
        {
            throw new InvalidRequestException("pageIndex must be 0 or greater.");
        }

        if (pageSize is < 1 || pageSize > maxPageSize)
        {
            throw new InvalidRequestException($"pageSize must be between 1 and {maxPageSize}.");
        }
    }

    private static InventoryDto MapInventory(Inventory i) => new()
    {
        InventoryId = i.InventoryId,
        ProjectId = i.ProjectId,
        UnitTypeId = i.UnitTypeId,
        UnitNumber = i.UnitNumber,
        BuildingTower = i.BuildingTower,
        FloorNumber = i.FloorNumber,
        ViewType = i.ViewType,
        AreaSqFt = i.AreaSqFt,
        Bedrooms = i.Bedrooms,
        Bathrooms = i.Bathrooms,
        ParkingSpaces = i.ParkingSpaces,
        HasBalcony = i.HasBalcony,
        FurnishingStatus = i.FurnishingStatus,
        ListPrice = i.ListPrice,
        PricePerSqFt = i.PricePerSqFt,
        Status = i.Status,
        ListingDate = i.ListingDate,
        SoldOrLeasedDate = i.SoldOrLeasedDate,
        BuyerTenantName = i.BuyerTenantName,
        SalesAgentId = i.SalesAgentId,
        AgentName = i.AgentName,
        AgentContact = i.AgentContact,
        Notes = i.Notes,
        ApprovalStatus = i.ApprovalStatus
    };
}
