using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NapsterAI.Api.Data;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateLookupServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    [Fact]
    public async Task ListAsync_ReturnsEmpty_WhenNoLookups()
    {
        using var db = CreateContext();
        var service = new RealEstateLookupService(db, new MemoryCache(new MemoryCacheOptions()));

        var result = await service.ListAsync(null, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAsync_ReturnsAll_WhenNoFilterApplied()
    {
        using var db = CreateContext();
        db.Lookups.AddRange(
            new Lookup { LookupType = "ProjectType", Code = "Residential", DisplayName = "Residential", DisplayOrder = 1 },
            new Lookup { LookupType = "ProjectStatus", Code = "Planning", DisplayName = "Planning", DisplayOrder = 1 });
        await db.SaveChangesAsync();

        var service = new RealEstateLookupService(db, new MemoryCache(new MemoryCacheOptions()));
        var result = await service.ListAsync(null, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ListAsync_FiltersByLookupType()
    {
        using var db = CreateContext();
        db.Lookups.AddRange(
            new Lookup { LookupType = "ProjectType", Code = "Residential", DisplayName = "Residential", DisplayOrder = 1 },
            new Lookup { LookupType = "ProjectType", Code = "Commercial", DisplayName = "Commercial", DisplayOrder = 2 },
            new Lookup { LookupType = "ProjectStatus", Code = "Planning", DisplayName = "Planning", DisplayOrder = 1 });
        await db.SaveChangesAsync();

        var service = new RealEstateLookupService(db, new MemoryCache(new MemoryCacheOptions()));
        var result = await service.ListAsync("ProjectType", CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, l => Assert.Equal("ProjectType", l.LookupType));
    }

    [Fact]
    public async Task ListAsync_CachesResult_AcrossCalls()
    {
        using var db = CreateContext();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new RealEstateLookupService(db, cache);

        var first = await service.ListAsync("ProjectType", CancellationToken.None);
        Assert.Empty(first);

        // Bypass the service (and its cache) to add data directly - a second call with the
        // same filter should still return the stale cached (empty) result.
        db.Lookups.Add(new Lookup { LookupType = "ProjectType", Code = "Residential", DisplayName = "Residential" });
        await db.SaveChangesAsync();

        var second = await service.ListAsync("ProjectType", CancellationToken.None);
        Assert.Empty(second);
    }
}
