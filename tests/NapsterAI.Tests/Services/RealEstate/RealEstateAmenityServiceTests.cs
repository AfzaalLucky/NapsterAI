using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateAmenityServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static async Task<int> SeedProjectAsync(RealEstateDbContext db)
    {
        var project = new Project
        {
            ProjectCode = $"PRJ-{Guid.NewGuid():N}", ProjectName = "Test Project", ProjectType = "Residential",
            Status = "Planning", Country = "UAE", City = "Dubai", Currency = "AED"
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project.ProjectId;
    }

    private static CreateAmenityRequestDto ValidRequest(int projectId) => new()
    {
        ProjectId = projectId, AmenityName = "Rooftop Pool", Category = "Recreational"
    };

    [Fact]
    public async Task CreateAsync_Throws_WhenAmenityNameMissing()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateAmenityService(db, NullLogger<RealEstateAmenityService>.Instance);

        var request = ValidRequest(projectId);
        request.AmenityName = "";

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCategoryInvalid()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateAmenityService(db, NullLogger<RealEstateAmenityService>.Instance);

        var request = ValidRequest(projectId);
        request.Category = "Bogus";

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenProjectIdUnknown()
    {
        using var db = CreateContext();
        var service = new RealEstateAmenityService(db, NullLogger<RealEstateAmenityService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(ValidRequest(999), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_WhenValid()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateAmenityService(db, NullLogger<RealEstateAmenityService>.Instance);

        var result = await service.CreateAsync(ValidRequest(projectId), CancellationToken.None);

        Assert.True(result.AmenityId > 0);
        Assert.Equal("Rooftop Pool", result.AmenityName);
        Assert.Equal(projectId, result.ProjectId);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateAmenityService(db, NullLogger<RealEstateAmenityService>.Instance);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateAmenityService(db, NullLogger<RealEstateAmenityService>.Instance);
        var created = await service.CreateAsync(ValidRequest(projectId), CancellationToken.None);

        var update = new UpdateAmenityRequestDto
        {
            ProjectId = projectId, AmenityName = "Infinity Pool", Category = "Wellness", IsHighlighted = true
        };
        var updated = await service.UpdateAsync(created.AmenityId, update, CancellationToken.None);

        Assert.Equal("Infinity Pool", updated.AmenityName);
        Assert.Equal("Wellness", updated.Category);
        Assert.True(updated.IsHighlighted);
    }

    [Fact]
    public async Task DeleteAsync_Succeeds_AndRemovesAmenity()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateAmenityService(db, NullLogger<RealEstateAmenityService>.Instance);
        var created = await service.CreateAsync(ValidRequest(projectId), CancellationToken.None);

        await service.DeleteAsync(created.AmenityId, CancellationToken.None);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(created.AmenityId, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateAmenityService(db, NullLogger<RealEstateAmenityService>.Instance);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.DeleteAsync(999, CancellationToken.None));
    }
}
