using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateUnitTypeServiceTests
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

    private static CreateUnitTypeRequestDto ValidRequest(int projectId) => new()
    {
        ProjectId = projectId, TypeName = "1 Bedroom", Category = "Apartment"
    };

    [Fact]
    public async Task CreateAsync_Throws_WhenTypeNameMissing()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);

        var request = ValidRequest(projectId);
        request.TypeName = "";

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCategoryInvalid()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);

        var request = ValidRequest(projectId);
        request.Category = "Bogus";

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenProjectIdUnknown()
    {
        using var db = CreateContext();
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(ValidRequest(999), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenMinAreaGreaterThanMaxArea()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);

        var request = ValidRequest(projectId);
        request.MinAreaSqFt = 1000;
        request.MaxAreaSqFt = 500;

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_WhenValid()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);

        var result = await service.CreateAsync(ValidRequest(projectId), CancellationToken.None);

        Assert.True(result.UnitTypeId > 0);
        Assert.Equal("1 Bedroom", result.TypeName);
        Assert.Equal(projectId, result.ProjectId);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);
        var created = await service.CreateAsync(ValidRequest(projectId), CancellationToken.None);

        var update = new UpdateUnitTypeRequestDto
        {
            ProjectId = projectId, TypeName = "2 Bedroom", Category = "Apartment", Bedrooms = 2
        };
        var updated = await service.UpdateAsync(created.UnitTypeId, update, CancellationToken.None);

        Assert.Equal("2 Bedroom", updated.TypeName);
        Assert.Equal(2, updated.Bedrooms);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenInventoryStillReferences()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);
        var created = await service.CreateAsync(ValidRequest(projectId), CancellationToken.None);

        db.Inventory.Add(new Inventory
        {
            ProjectId = projectId, UnitTypeId = created.UnitTypeId, UnitNumber = "101", Status = "Available"
        });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<RealEstateConflictException>(
            () => service.DeleteAsync(created.UnitTypeId, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_Succeeds_WhenNoInventoryReferences()
    {
        using var db = CreateContext();
        var projectId = await SeedProjectAsync(db);
        var service = new RealEstateUnitTypeService(db, NullLogger<RealEstateUnitTypeService>.Instance);
        var created = await service.CreateAsync(ValidRequest(projectId), CancellationToken.None);

        await service.DeleteAsync(created.UnitTypeId, CancellationToken.None);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(created.UnitTypeId, CancellationToken.None));
    }
}
