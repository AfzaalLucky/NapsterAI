using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateInventoryServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static async Task<(Project project, UnitType unitType)> SeedProjectAndUnitTypeAsync(RealEstateDbContext db)
    {
        var project = new Project
        {
            ProjectCode = "PRJ-SEED", ProjectName = "Seed Project", ProjectType = "Residential",
            Status = "Ready", Country = "UAE", City = "Dubai", Currency = "AED"
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var unitType = new UnitType { ProjectId = project.ProjectId, TypeName = "2 Bedroom", Category = "Apartment", Bedrooms = 2 };
        db.UnitTypes.Add(unitType);
        await db.SaveChangesAsync();

        return (project, unitType);
    }

    private static CreateInventoryRequestDto ValidRequest(int projectId, int unitTypeId, string unitNumber, decimal price, int bedrooms) => new()
    {
        ProjectId = projectId,
        UnitTypeId = unitTypeId,
        UnitNumber = unitNumber,
        Status = RealEstateInventoryStatuses.Available,
        ListPrice = price,
        Bedrooms = bedrooms
    };

    [Fact]
    public async Task CreateAsync_Throws_WhenUnitTypeDoesNotBelongToProject()
    {
        using var db = CreateContext();
        var (project, unitType) = await SeedProjectAndUnitTypeAsync(db);

        var otherProject = new Project { ProjectCode = "PRJ-OTHER", ProjectName = "Other", ProjectType = "Residential", Status = "Ready", Country = "UAE", City = "Dubai", Currency = "AED" };
        db.Projects.Add(otherProject);
        await db.SaveChangesAsync();

        var service = new RealEstateInventoryService(db, NullLogger<RealEstateInventoryService>.Instance);
        var request = ValidRequest(otherProject.ProjectId, unitType.UnitTypeId, "X-1", 1000000m, 2);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenUnitNumberAlreadyExistsInProject()
    {
        using var db = CreateContext();
        var (project, unitType) = await SeedProjectAndUnitTypeAsync(db);
        var service = new RealEstateInventoryService(db, NullLogger<RealEstateInventoryService>.Instance);

        await service.CreateAsync(ValidRequest(project.ProjectId, unitType.UnitTypeId, "A-101", 1000000m, 2), CancellationToken.None);

        await Assert.ThrowsAsync<RealEstateConflictException>(
            () => service.CreateAsync(ValidRequest(project.ProjectId, unitType.UnitTypeId, "A-101", 1200000m, 2), CancellationToken.None));
    }

    [Fact]
    public async Task ListAsync_FiltersByBedroomsAndPriceRange_AndSortsByPriceDescending()
    {
        using var db = CreateContext();
        var (project, unitType) = await SeedProjectAndUnitTypeAsync(db);
        var service = new RealEstateInventoryService(db, NullLogger<RealEstateInventoryService>.Instance);

        await service.CreateAsync(ValidRequest(project.ProjectId, unitType.UnitTypeId, "CHEAP-1BR", 800000m, 1), CancellationToken.None);
        await service.CreateAsync(ValidRequest(project.ProjectId, unitType.UnitTypeId, "MID-2BR", 1500000m, 2), CancellationToken.None);
        await service.CreateAsync(ValidRequest(project.ProjectId, unitType.UnitTypeId, "EXPENSIVE-2BR", 5000000m, 2), CancellationToken.None);

        var result = await service.ListAsync(
            projectId: null, unitTypeId: null, minBedrooms: 2, maxBedrooms: null,
            minPrice: null, maxPrice: 2000000m, minAreaSqFt: null, maxAreaSqFt: null,
            status: null, viewType: null, furnishingStatus: null, search: null,
            sortBy: "price", sortDescending: true, pageIndex: 0, pageSize: 20, cancellationToken: CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("MID-2BR", result.Items[0].UnitNumber);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_Throws_WhenSortByInvalid()
    {
        using var db = CreateContext();
        var service = new RealEstateInventoryService(db, NullLogger<RealEstateInventoryService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.ListAsync(
            null, null, null, null, null, null, null, null,
            null, null, null, null, "alphabetical", false, 0, 20, CancellationToken.None));
    }
}
