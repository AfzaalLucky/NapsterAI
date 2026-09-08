using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// CRUD + approval workflow for Projects. Follows the same validate-before-write,
/// static-allow-list idiom as Services\NapsterService.cs, but against local EF Core
/// persistence instead of an upstream HTTP API.
/// </summary>
public class RealEstateProjectService : IRealEstateProjectService
{
    private readonly RealEstateDbContext _db;
    private readonly ILogger<RealEstateProjectService> _logger;

    public RealEstateProjectService(RealEstateDbContext db, ILogger<RealEstateProjectService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResultDto<ProjectDto>> ListAsync(
        string? city, string? projectType, string? status, bool? isFeatured,
        decimal? minPrice, decimal? maxPrice, string? search,
        int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        var query = _db.Projects.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(p => p.City == city);
        if (!string.IsNullOrWhiteSpace(projectType)) query = query.Where(p => p.ProjectType == projectType);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        if (isFeatured is not null) query = query.Where(p => p.IsFeatured == isFeatured);
        if (minPrice is not null) query = query.Where(p => p.StartingPrice >= minPrice);
        if (maxPrice is not null) query = query.Where(p => p.MaxPrice <= maxPrice || p.MaxPrice == null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.ProjectName.Contains(search) || p.ProjectCode.Contains(search));
        }

        var totalCount = await _db.Projects.CountAsync(cancellationToken);
        var filteredCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.ProjectName)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(p => MapProject(p))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ProjectDto>
        {
            Items = items,
            TotalCount = totalCount,
            FilteredCount = filteredCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<ProjectDto> GetByIdAsync(int projectId, CancellationToken cancellationToken)
    {
        var project = await FindAsync(projectId, cancellationToken);
        return MapProject(project);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequestDto request, CancellationToken cancellationToken)
    {
        ValidateWrite(request);

        if (await _db.Projects.AnyAsync(p => p.ProjectCode == request.ProjectCode, cancellationToken))
        {
            throw new RealEstateConflictException($"A project with code '{request.ProjectCode}' already exists.");
        }

        var project = new Project { ApprovalStatus = RealEstateApprovalStatuses.Draft, IsActive = true };
        ApplyToEntity(request, project);

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created Real Estate project {ProjectId} ({ProjectCode})", project.ProjectId, project.ProjectCode);

        return MapProject(project);
    }

    public async Task<ProjectDto> UpdateAsync(int projectId, UpdateProjectRequestDto request, CancellationToken cancellationToken)
    {
        ValidateWrite(request);

        var project = await FindAsync(projectId, cancellationToken);

        if (!string.Equals(project.ProjectCode, request.ProjectCode, StringComparison.Ordinal)
            && await _db.Projects.AnyAsync(p => p.ProjectCode == request.ProjectCode && p.ProjectId != projectId, cancellationToken))
        {
            throw new RealEstateConflictException($"A project with code '{request.ProjectCode}' already exists.");
        }

        ApplyToEntity(request, project);
        project.IsActive = request.IsActive;
        project.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated Real Estate project {ProjectId}", project.ProjectId);

        return MapProject(project);
    }

    public async Task DeleteAsync(int projectId, CancellationToken cancellationToken)
    {
        var project = await FindAsync(projectId, cancellationToken);

        // Soft delete only - IsDeleted drives the query filter on Project and its children
        // (see ProjectConfiguration/UnitTypeConfiguration/InventoryConfiguration/AmenityConfiguration).
        project.IsDeleted = true;
        project.IsActive = false;
        project.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Soft-deleted Real Estate project {ProjectId}", project.ProjectId);
    }

    public async Task<ProjectDto> ApproveAsync(int projectId, CancellationToken cancellationToken)
    {
        var project = await FindAsync(projectId, cancellationToken);
        project.ApprovalStatus = RealEstateApprovalStatuses.Approved;
        project.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return MapProject(project);
    }

    public async Task<ProjectDto> RejectAsync(int projectId, CancellationToken cancellationToken)
    {
        var project = await FindAsync(projectId, cancellationToken);
        project.ApprovalStatus = RealEstateApprovalStatuses.Rejected;
        project.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return MapProject(project);
    }

    public async Task<IReadOnlyList<UnitTypeDto>> GetUnitTypesAsync(int projectId, CancellationToken cancellationToken)
    {
        await FindAsync(projectId, cancellationToken);

        return await _db.UnitTypes.AsNoTracking()
            .Where(u => u.ProjectId == projectId)
            .Select(u => MapUnitType(u))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AmenityDto>> GetAmenitiesAsync(int projectId, CancellationToken cancellationToken)
    {
        await FindAsync(projectId, cancellationToken);

        return await _db.Amenities.AsNoTracking()
            .Where(a => a.ProjectId == projectId)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => MapAmenity(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MediaDto>> GetMediaAsync(int projectId, CancellationToken cancellationToken)
    {
        await FindAsync(projectId, cancellationToken);

        return await _db.Media.AsNoTracking()
            .Where(m => m.EntityType == RealEstateMediaEntityTypes.Project && m.EntityId == projectId)
            .OrderBy(m => m.DisplayOrder)
            .Select(m => MapMedia(m))
            .ToListAsync(cancellationToken);
    }

    private async Task<Project> FindAsync(int projectId, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);
        if (project is null)
        {
            throw new RealEstateResourceNotFoundException($"Project {projectId} was not found.");
        }

        return project;
    }

    private static void ValidateWrite(CreateProjectRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectCode))
        {
            throw new InvalidRequestException("projectCode is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ProjectName))
        {
            throw new InvalidRequestException("projectName is required.");
        }

        if (!RealEstateProjectTypes.All.Contains(request.ProjectType))
        {
            throw new InvalidRequestException($"projectType must be one of: {string.Join(", ", RealEstateProjectTypes.All)}.");
        }

        if (!RealEstateProjectStatuses.All.Contains(request.Status))
        {
            throw new InvalidRequestException($"status must be one of: {string.Join(", ", RealEstateProjectStatuses.All)}.");
        }

        if (string.IsNullOrWhiteSpace(request.Country))
        {
            throw new InvalidRequestException("country is required.");
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            throw new InvalidRequestException("city is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            throw new InvalidRequestException("currency is required.");
        }

        if (request.StartingPrice is not null && request.MaxPrice is not null && request.StartingPrice > request.MaxPrice)
        {
            throw new InvalidRequestException("startingPrice must not be greater than maxPrice.");
        }
    }

    private static void ApplyToEntity(CreateProjectRequestDto request, Project project)
    {
        project.ProjectCode = request.ProjectCode;
        project.ProjectName = request.ProjectName;
        project.Developer = request.Developer;
        project.ProjectType = request.ProjectType;
        project.Status = request.Status;
        project.Description = request.Description;
        project.Country = request.Country;
        project.City = request.City;
        project.District = request.District;
        project.Address = request.Address;
        project.Latitude = request.Latitude;
        project.Longitude = request.Longitude;
        project.TotalBuildings = request.TotalBuildings;
        project.TotalFloors = request.TotalFloors;
        project.TotalUnits = request.TotalUnits;
        project.LaunchDate = request.LaunchDate;
        project.ConstructionStart = request.ConstructionStart;
        project.EstimatedCompletion = request.EstimatedCompletion;
        project.HandoverDate = request.HandoverDate;
        project.StartingPrice = request.StartingPrice;
        project.MaxPrice = request.MaxPrice;
        project.Currency = request.Currency;
        project.PaymentPlan = request.PaymentPlan;
        project.PermitNumber = request.PermitNumber;
        project.ServiceCharge = request.ServiceCharge;
        project.MasterPlanUrl = request.MasterPlanUrl;
        project.BrochureUrl = request.BrochureUrl;
        project.ImageUrl = request.ImageUrl;
        project.VideoUrl = request.VideoUrl;
        project.ContactPerson = request.ContactPerson;
        project.ContactPhone = request.ContactPhone;
        project.ContactEmail = request.ContactEmail;
        project.IsFeatured = request.IsFeatured;
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

    internal static ProjectDto MapProject(Project p) => new()
    {
        ProjectId = p.ProjectId,
        ProjectCode = p.ProjectCode,
        ProjectName = p.ProjectName,
        Developer = p.Developer,
        ProjectType = p.ProjectType,
        Status = p.Status,
        Description = p.Description,
        Country = p.Country,
        City = p.City,
        District = p.District,
        Address = p.Address,
        Latitude = p.Latitude,
        Longitude = p.Longitude,
        TotalBuildings = p.TotalBuildings,
        TotalFloors = p.TotalFloors,
        TotalUnits = p.TotalUnits,
        LaunchDate = p.LaunchDate,
        ConstructionStart = p.ConstructionStart,
        EstimatedCompletion = p.EstimatedCompletion,
        HandoverDate = p.HandoverDate,
        StartingPrice = p.StartingPrice,
        MaxPrice = p.MaxPrice,
        Currency = p.Currency,
        PaymentPlan = p.PaymentPlan,
        PermitNumber = p.PermitNumber,
        ServiceCharge = p.ServiceCharge,
        MasterPlanUrl = p.MasterPlanUrl,
        BrochureUrl = p.BrochureUrl,
        ImageUrl = p.ImageUrl,
        VideoUrl = p.VideoUrl,
        ContactPerson = p.ContactPerson,
        ContactPhone = p.ContactPhone,
        ContactEmail = p.ContactEmail,
        IsFeatured = p.IsFeatured,
        IsActive = p.IsActive,
        ApprovalStatus = p.ApprovalStatus,
        CreatedDate = p.CreatedDate,
        ModifiedDate = p.ModifiedDate
    };

    internal static UnitTypeDto MapUnitType(UnitType u) => new()
    {
        UnitTypeId = u.UnitTypeId,
        ProjectId = u.ProjectId,
        TypeName = u.TypeName,
        Category = u.Category,
        Bedrooms = u.Bedrooms,
        Bathrooms = u.Bathrooms,
        MinAreaSqFt = u.MinAreaSqFt,
        MaxAreaSqFt = u.MaxAreaSqFt,
        BasePrice = u.BasePrice,
        PricePerSqFt = u.PricePerSqFt,
        TotalUnitsOfType = u.TotalUnitsOfType,
        AvailableUnitsOfType = u.AvailableUnitsOfType,
        FloorPlanUrl = u.FloorPlanUrl,
        Description = u.Description
    };

    internal static AmenityDto MapAmenity(Amenity a) => new()
    {
        AmenityId = a.AmenityId,
        ProjectId = a.ProjectId,
        AmenityName = a.AmenityName,
        Category = a.Category,
        Description = a.Description,
        IconUrl = a.IconUrl,
        ImageUrl = a.ImageUrl,
        IsHighlighted = a.IsHighlighted,
        DisplayOrder = a.DisplayOrder
    };

    internal static MediaDto MapMedia(Media m) => new()
    {
        MediaId = m.MediaId,
        EntityType = m.EntityType,
        EntityId = m.EntityId,
        MediaType = m.MediaType,
        Url = m.Url,
        DisplayOrder = m.DisplayOrder,
        IsPrimary = m.IsPrimary
    };
}
