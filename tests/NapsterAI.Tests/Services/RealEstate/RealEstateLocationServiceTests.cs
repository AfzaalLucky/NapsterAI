using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NapsterAI.Api.Data;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateLocationServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    [Fact]
    public async Task ListAsync_ReturnsEmpty_WhenNoLocations()
    {
        using var db = CreateContext();
        var service = new RealEstateLocationService(db, new MemoryCache(new MemoryCacheOptions()));

        var result = await service.ListAsync(null, null, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAsync_ReturnsAll_WhenNoFilterApplied()
    {
        using var db = CreateContext();
        db.Locations.AddRange(
            new Location { Country = "UAE", City = "Dubai", District = "Marina" },
            new Location { Country = "UAE", City = "Abu Dhabi", District = "Downtown" });
        await db.SaveChangesAsync();

        var service = new RealEstateLocationService(db, new MemoryCache(new MemoryCacheOptions()));
        var result = await service.ListAsync(null, null, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ListAsync_FiltersByCountryAndCity()
    {
        using var db = CreateContext();
        db.Locations.AddRange(
            new Location { Country = "UAE", City = "Dubai", District = "Marina" },
            new Location { Country = "UAE", City = "Abu Dhabi", District = "Downtown" },
            new Location { Country = "Qatar", City = "Doha", District = "West Bay" });
        await db.SaveChangesAsync();

        var service = new RealEstateLocationService(db, new MemoryCache(new MemoryCacheOptions()));
        var result = await service.ListAsync("UAE", "Dubai", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Marina", result[0].District);
    }

    [Fact]
    public async Task ListAsync_CachesResult_AcrossCalls()
    {
        using var db = CreateContext();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new RealEstateLocationService(db, cache);

        var first = await service.ListAsync(null, null, CancellationToken.None);
        Assert.Empty(first);

        // Bypass the service (and its cache) to add data directly - a second call with the
        // same filter should still return the stale cached (empty) result.
        db.Locations.Add(new Location { Country = "UAE", City = "Dubai" });
        await db.SaveChangesAsync();

        var second = await service.ListAsync(null, null, CancellationToken.None);
        Assert.Empty(second);
    }
}
