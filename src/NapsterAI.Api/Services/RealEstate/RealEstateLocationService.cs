using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NapsterAI.Api.Data;
using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// Read-only lookup used to populate country/city/district filter dropdowns and
/// autocomplete on the public site and admin dashboard (see RealEstateImplementationPlan.md §4.2).
/// Cached in-memory since this data changes rarely.
/// </summary>
public class RealEstateLocationService : IRealEstateLocationService
{
    private const string CacheKeyPrefix = "RealEstate:Locations:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private readonly RealEstateDbContext _db;
    private readonly IMemoryCache _cache;

    public RealEstateLocationService(RealEstateDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IReadOnlyList<LocationDto>> ListAsync(string? country, string? city, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{country ?? "*"}:{city ?? "*"}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<LocationDto>? cached) && cached is not null)
        {
            return cached;
        }

        var query = _db.Locations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(country)) query = query.Where(l => l.Country == country);
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(l => l.City == city);

        var items = await query
            .OrderBy(l => l.Country).ThenBy(l => l.City).ThenBy(l => l.District)
            .Select(l => new LocationDto
            {
                LocationId = l.LocationId,
                Country = l.Country,
                City = l.City,
                District = l.District,
                Latitude = l.Latitude,
                Longitude = l.Longitude
            })
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, items, CacheDuration);

        return items;
    }
}
