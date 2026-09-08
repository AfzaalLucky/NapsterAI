using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NapsterAI.Api.Data;
using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// Read-only reference data backing admin UI dropdowns. Cached in-memory since this data
/// changes rarely (see RealEstateImplementationPlan.md §4.5: "IMemoryCache for rarely-changing
/// reads: Lookups, Locations, per-project Amenities").
/// </summary>
public class RealEstateLookupService : IRealEstateLookupService
{
    private const string CacheKeyPrefix = "RealEstate:Lookups:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private readonly RealEstateDbContext _db;
    private readonly IMemoryCache _cache;

    public RealEstateLookupService(RealEstateDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IReadOnlyList<LookupDto>> ListAsync(string? lookupType, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{lookupType ?? "*"}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<LookupDto>? cached) && cached is not null)
        {
            return cached;
        }

        var query = _db.Lookups.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(lookupType)) query = query.Where(l => l.LookupType == lookupType);

        var items = await query
            .OrderBy(l => l.LookupType).ThenBy(l => l.DisplayOrder)
            .Select(l => new LookupDto
            {
                LookupId = l.LookupId,
                LookupType = l.LookupType,
                Code = l.Code,
                DisplayName = l.DisplayName,
                DisplayOrder = l.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, items, CacheDuration);

        return items;
    }
}
