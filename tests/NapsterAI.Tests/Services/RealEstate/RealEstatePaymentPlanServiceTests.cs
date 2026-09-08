using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstatePaymentPlanServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static async Task<Project> SeedProjectAsync(RealEstateDbContext db)
    {
        var project = new Project
        {
            ProjectCode = "PRJ-100", ProjectName = "Test", ProjectType = "Residential",
            Status = "Ready", Country = "UAE", City = "Dubai", Currency = "AED"
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project;
    }

    private static async Task<Inventory> SeedInventoryAsync(RealEstateDbContext db, Project project, decimal? listPrice = 1_000_000m)
    {
        var inventory = new Inventory
        {
            ProjectId = project.ProjectId, UnitTypeId = 1, UnitNumber = "A-101",
            Status = RealEstateInventoryStatuses.Available, ListPrice = listPrice
        };
        db.Inventory.Add(inventory);
        await db.SaveChangesAsync();
        inventory.Project = project;
        return inventory;
    }

    [Fact]
    public async Task ListMilestonesAsync_Throws_WhenProjectNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstatePaymentPlanService(db);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.ListMilestonesAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task AddMilestoneAsync_Throws_WhenPercentDueOutOfRange()
    {
        using var db = CreateContext();
        var project = await SeedProjectAsync(db);
        var service = new RealEstatePaymentPlanService(db);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.AddMilestoneAsync(
            project.ProjectId,
            new CreatePaymentPlanMilestoneRequestDto { MilestoneName = "Booking", PercentDue = 150 },
            CancellationToken.None));
    }

    [Fact]
    public async Task AddMilestoneAsync_Succeeds()
    {
        using var db = CreateContext();
        var project = await SeedProjectAsync(db);
        var service = new RealEstatePaymentPlanService(db);

        var result = await service.AddMilestoneAsync(
            project.ProjectId,
            new CreatePaymentPlanMilestoneRequestDto { MilestoneName = "Booking", PercentDue = 20, DisplayOrder = 1 },
            CancellationToken.None);

        Assert.True(result.MilestoneId > 0);
        Assert.Equal("Booking", result.MilestoneName);
    }

    [Fact]
    public async Task CalculateAsync_Throws_WhenInventoryNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstatePaymentPlanService(db);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.CalculateAsync(999, new CalculatePaymentPlanRequestDto(), CancellationToken.None));
    }

    [Fact]
    public async Task CalculateAsync_Throws_WhenListPriceMissing()
    {
        using var db = CreateContext();
        var project = await SeedProjectAsync(db);
        var inventory = await SeedInventoryAsync(db, project, listPrice: null);
        var service = new RealEstatePaymentPlanService(db);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CalculateAsync(inventory.InventoryId, new CalculatePaymentPlanRequestDto(), CancellationToken.None));
    }

    [Fact]
    public async Task CalculateAsync_Throws_WhenNoMilestonesConfigured()
    {
        using var db = CreateContext();
        var project = await SeedProjectAsync(db);
        var inventory = await SeedInventoryAsync(db, project);
        var service = new RealEstatePaymentPlanService(db);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.CalculateAsync(inventory.InventoryId, new CalculatePaymentPlanRequestDto(), CancellationToken.None));
    }

    [Fact]
    public async Task CalculateAsync_ReturnsPlainSchedule_WhenNoDownPaymentRequested()
    {
        using var db = CreateContext();
        var project = await SeedProjectAsync(db);
        var inventory = await SeedInventoryAsync(db, project);
        var service = new RealEstatePaymentPlanService(db);
        await service.AddMilestoneAsync(project.ProjectId, new CreatePaymentPlanMilestoneRequestDto { MilestoneName = "Booking", PercentDue = 30, DisplayOrder = 1 }, CancellationToken.None);
        await service.AddMilestoneAsync(project.ProjectId, new CreatePaymentPlanMilestoneRequestDto { MilestoneName = "Handover", PercentDue = 70, DisplayOrder = 2 }, CancellationToken.None);

        var schedule = await service.CalculateAsync(inventory.InventoryId, new CalculatePaymentPlanRequestDto(), CancellationToken.None);

        Assert.Equal(2, schedule.Milestones.Count);
        Assert.Equal(300_000m, schedule.Milestones[0].AmountDue);
        Assert.Equal(700_000m, schedule.Milestones[1].AmountDue);
    }

    [Fact]
    public async Task CalculateAsync_ScalesRemainingMilestones_WhenDownPaymentRequested()
    {
        using var db = CreateContext();
        var project = await SeedProjectAsync(db);
        var inventory = await SeedInventoryAsync(db, project);
        var service = new RealEstatePaymentPlanService(db);
        await service.AddMilestoneAsync(project.ProjectId, new CreatePaymentPlanMilestoneRequestDto { MilestoneName = "Booking", PercentDue = 30, DisplayOrder = 1 }, CancellationToken.None);
        await service.AddMilestoneAsync(project.ProjectId, new CreatePaymentPlanMilestoneRequestDto { MilestoneName = "Handover", PercentDue = 70, DisplayOrder = 2 }, CancellationToken.None);

        var schedule = await service.CalculateAsync(
            inventory.InventoryId, new CalculatePaymentPlanRequestDto { DownPaymentPercent = 10 }, CancellationToken.None);

        Assert.Equal(3, schedule.Milestones.Count);
        Assert.Equal("Down Payment", schedule.Milestones[0].MilestoneName);
        Assert.Equal(100_000m, schedule.Milestones[0].AmountDue);

        var totalPercent = schedule.Milestones.Sum(m => m.PercentDue);
        Assert.Equal(100m, totalPercent);
    }
}
