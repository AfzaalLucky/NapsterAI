using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public class RealEstateUnitTypeService : IRealEstateUnitTypeService
{
    private readonly RealEstateDbContext _db;
    private readonly ILogger<RealEstateUnitTypeService> _logger;

    public RealEstateUnitTypeService(RealEstateDbContext db, ILogger<RealEstateUnitTypeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UnitTypeDto>> ListAsync(int? projectId, CancellationToken cancellationToken)
    {
        var query = _db.UnitTypes.AsNoTracking().AsQueryable();
        if (projectId is not null) query = query.Where(u => u.ProjectId == projectId);

        return await query
            .OrderBy(u => u.ProjectId).ThenBy(u => u.TypeName)
            .Select(u => RealEstateProjectService.MapUnitType(u))
            .ToListAsync(cancellationToken);
    }

    public async Task<UnitTypeDto> GetByIdAsync(int unitTypeId, CancellationToken cancellationToken)
    {
        var unitType = await FindAsync(unitTypeId, cancellationToken);
        return RealEstateProjectService.MapUnitType(unitType);
    }

    public async Task<UnitTypeDto> CreateAsync(CreateUnitTypeRequestDto request, CancellationToken cancellationToken)
    {
        await ValidateWriteAsync(request, cancellationToken);

        var unitType = new UnitType();
        ApplyToEntity(request, unitType);

        _db.UnitTypes.Add(unitType);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created unit type {UnitTypeId} for project {ProjectId}", unitType.UnitTypeId, unitType.ProjectId);

        return RealEstateProjectService.MapUnitType(unitType);
    }

    public async Task<UnitTypeDto> UpdateAsync(int unitTypeId, UpdateUnitTypeRequestDto request, CancellationToken cancellationToken)
    {
        await ValidateWriteAsync(request, cancellationToken);

        var unitType = await FindAsync(unitTypeId, cancellationToken);
        ApplyToEntity(request, unitType);
        unitType.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return RealEstateProjectService.MapUnitType(unitType);
    }

    public async Task DeleteAsync(int unitTypeId, CancellationToken cancellationToken)
    {
        var unitType = await FindAsync(unitTypeId, cancellationToken);

        var hasInventory = await _db.Inventory.AnyAsync(i => i.UnitTypeId == unitTypeId, cancellationToken);
        if (hasInventory)
        {
            throw new RealEstateConflictException($"Unit type {unitTypeId} still has inventory units and cannot be deleted.");
        }

        _db.UnitTypes.Remove(unitType);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<UnitType> FindAsync(int unitTypeId, CancellationToken cancellationToken)
    {
        var unitType = await _db.UnitTypes.FirstOrDefaultAsync(u => u.UnitTypeId == unitTypeId, cancellationToken);
        if (unitType is null)
        {
            throw new RealEstateResourceNotFoundException($"Unit type {unitTypeId} was not found.");
        }

        return unitType;
    }

    private async Task ValidateWriteAsync(CreateUnitTypeRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TypeName))
        {
            throw new InvalidRequestException("typeName is required.");
        }

        if (!RealEstateUnitCategories.All.Contains(request.Category))
        {
            throw new InvalidRequestException($"category must be one of: {string.Join(", ", RealEstateUnitCategories.All)}.");
        }

        if (!await _db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId, cancellationToken))
        {
            throw new InvalidRequestException($"projectId {request.ProjectId} does not reference an existing project.");
        }

        if (request.MinAreaSqFt is not null && request.MaxAreaSqFt is not null && request.MinAreaSqFt > request.MaxAreaSqFt)
        {
            throw new InvalidRequestException("minAreaSqFt must not be greater than maxAreaSqFt.");
        }
    }

    private static void ApplyToEntity(CreateUnitTypeRequestDto request, UnitType unitType)
    {
        unitType.ProjectId = request.ProjectId;
        unitType.TypeName = request.TypeName;
        unitType.Category = request.Category;
        unitType.Bedrooms = request.Bedrooms;
        unitType.Bathrooms = request.Bathrooms;
        unitType.MinAreaSqFt = request.MinAreaSqFt;
        unitType.MaxAreaSqFt = request.MaxAreaSqFt;
        unitType.BasePrice = request.BasePrice;
        unitType.PricePerSqFt = request.PricePerSqFt;
        unitType.TotalUnitsOfType = request.TotalUnitsOfType;
        unitType.AvailableUnitsOfType = request.AvailableUnitsOfType;
        unitType.FloorPlanUrl = request.FloorPlanUrl;
        unitType.Description = request.Description;
    }
}
