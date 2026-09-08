using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public class RealEstateAmenityService : IRealEstateAmenityService
{
    private readonly RealEstateDbContext _db;
    private readonly ILogger<RealEstateAmenityService> _logger;

    public RealEstateAmenityService(RealEstateDbContext db, ILogger<RealEstateAmenityService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AmenityDto>> ListAsync(int? projectId, string? category, CancellationToken cancellationToken)
    {
        var query = _db.Amenities.AsNoTracking().AsQueryable();
        if (projectId is not null) query = query.Where(a => a.ProjectId == projectId);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(a => a.Category == category);

        return await query
            .OrderBy(a => a.ProjectId).ThenBy(a => a.DisplayOrder)
            .Select(a => RealEstateProjectService.MapAmenity(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<AmenityDto> GetByIdAsync(int amenityId, CancellationToken cancellationToken)
    {
        var amenity = await FindAsync(amenityId, cancellationToken);
        return RealEstateProjectService.MapAmenity(amenity);
    }

    public async Task<AmenityDto> CreateAsync(CreateAmenityRequestDto request, CancellationToken cancellationToken)
    {
        await ValidateWriteAsync(request, cancellationToken);

        var amenity = new Amenity();
        ApplyToEntity(request, amenity);

        _db.Amenities.Add(amenity);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created amenity {AmenityId} for project {ProjectId}", amenity.AmenityId, amenity.ProjectId);

        return RealEstateProjectService.MapAmenity(amenity);
    }

    public async Task<AmenityDto> UpdateAsync(int amenityId, UpdateAmenityRequestDto request, CancellationToken cancellationToken)
    {
        await ValidateWriteAsync(request, cancellationToken);

        var amenity = await FindAsync(amenityId, cancellationToken);
        ApplyToEntity(request, amenity);
        amenity.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return RealEstateProjectService.MapAmenity(amenity);
    }

    public async Task DeleteAsync(int amenityId, CancellationToken cancellationToken)
    {
        var amenity = await FindAsync(amenityId, cancellationToken);
        _db.Amenities.Remove(amenity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Amenity> FindAsync(int amenityId, CancellationToken cancellationToken)
    {
        var amenity = await _db.Amenities.FirstOrDefaultAsync(a => a.AmenityId == amenityId, cancellationToken);
        if (amenity is null)
        {
            throw new RealEstateResourceNotFoundException($"Amenity {amenityId} was not found.");
        }

        return amenity;
    }

    private async Task ValidateWriteAsync(CreateAmenityRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AmenityName))
        {
            throw new InvalidRequestException("amenityName is required.");
        }

        if (!RealEstateAmenityCategories.All.Contains(request.Category))
        {
            throw new InvalidRequestException($"category must be one of: {string.Join(", ", RealEstateAmenityCategories.All)}.");
        }

        if (!await _db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId, cancellationToken))
        {
            throw new InvalidRequestException($"projectId {request.ProjectId} does not reference an existing project.");
        }
    }

    private static void ApplyToEntity(CreateAmenityRequestDto request, Amenity amenity)
    {
        amenity.ProjectId = request.ProjectId;
        amenity.AmenityName = request.AmenityName;
        amenity.Category = request.Category;
        amenity.Description = request.Description;
        amenity.IconUrl = request.IconUrl;
        amenity.ImageUrl = request.ImageUrl;
        amenity.IsHighlighted = request.IsHighlighted;
        amenity.DisplayOrder = request.DisplayOrder;
    }
}
