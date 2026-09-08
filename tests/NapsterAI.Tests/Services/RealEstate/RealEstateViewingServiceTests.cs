using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateViewingServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static async Task<int> SeedInventoryAsync(RealEstateDbContext db)
    {
        var project = new Project
        {
            ProjectCode = "PRJ-100",
            ProjectName = "Test Project",
            ProjectType = "Residential",
            Status = "Ready",
            Country = "UAE",
            City = "Dubai",
            Currency = "AED"
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // InMemory doesn't enforce the UnitType FK, so a bare id is fine here (matches
        // RealEstateProjectServiceTests's note on what InMemory does and doesn't enforce).
        var inventory = new Inventory
        {
            ProjectId = project.ProjectId,
            UnitTypeId = 1,
            UnitNumber = "A-101",
            Status = RealEstateInventoryStatuses.Available,
            ListPrice = 1_000_000m
        };
        db.Inventory.Add(inventory);
        await db.SaveChangesAsync();

        return inventory.InventoryId;
    }

    private static CreateViewingRequestDto ValidRequest(int inventoryId) => new()
    {
        CustomerName = "Jane Buyer",
        CustomerEmail = "jane@example.com",
        InventoryId = inventoryId,
        ScheduledDate = DateTime.UtcNow.AddDays(3)
    };

    [Fact]
    public async Task CreateAsync_Throws_WhenInventoryDoesNotExist()
    {
        using var db = CreateContext();
        var service = new RealEstateViewingService(db, NullLogger<RealEstateViewingService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateAsync(ValidRequest(999), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenScheduledDateInPast()
    {
        using var db = CreateContext();
        var inventoryId = await SeedInventoryAsync(db);
        var service = new RealEstateViewingService(db, NullLogger<RealEstateViewingService>.Instance);

        var request = ValidRequest(inventoryId);
        request.ScheduledDate = DateTime.UtcNow.AddDays(-1);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_AndCreatesLeadTransactionally()
    {
        using var db = CreateContext();
        var inventoryId = await SeedInventoryAsync(db);
        var service = new RealEstateViewingService(db, NullLogger<RealEstateViewingService>.Instance);

        var result = await service.CreateAsync(ValidRequest(inventoryId), CancellationToken.None);

        Assert.True(result.ViewingId > 0);
        Assert.Equal(RealEstateViewingStatuses.Requested, result.Status);
        Assert.Equal(1, await db.Leads.CountAsync());
        Assert.Equal(RealEstateLeadStatuses.ViewingScheduled, (await db.Leads.FirstAsync()).Status);
    }

    [Fact]
    public async Task CreateAsync_ReusesExistingNonTerminalLead_ForSameCustomerAndProject()
    {
        using var db = CreateContext();
        var inventoryId = await SeedInventoryAsync(db);
        var service = new RealEstateViewingService(db, NullLogger<RealEstateViewingService>.Instance);

        await service.CreateAsync(ValidRequest(inventoryId), CancellationToken.None);
        var request2 = ValidRequest(inventoryId);
        request2.ScheduledDate = DateTime.UtcNow.AddDays(5);
        await service.CreateAsync(request2, CancellationToken.None);

        Assert.Equal(1, await db.Leads.CountAsync());
        Assert.Equal(2, await db.Viewings.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateViewingService(db, NullLogger<RealEstateViewingService>.Instance);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_WhenStatusInvalid()
    {
        using var db = CreateContext();
        var inventoryId = await SeedInventoryAsync(db);
        var service = new RealEstateViewingService(db, NullLogger<RealEstateViewingService>.Instance);
        var viewing = await service.CreateAsync(ValidRequest(inventoryId), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.UpdateStatusAsync(
            viewing.ViewingId, new UpdateViewingStatusRequestDto { Status = "Bogus" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_ToCompleted_MovesLeadToNegotiation()
    {
        using var db = CreateContext();
        var inventoryId = await SeedInventoryAsync(db);
        var service = new RealEstateViewingService(db, NullLogger<RealEstateViewingService>.Instance);
        var viewing = await service.CreateAsync(ValidRequest(inventoryId), CancellationToken.None);

        var updated = await service.UpdateStatusAsync(
            viewing.ViewingId, new UpdateViewingStatusRequestDto { Status = RealEstateViewingStatuses.Completed }, CancellationToken.None);

        Assert.Equal(RealEstateViewingStatuses.Completed, updated.Status);
        var lead = await db.Leads.FirstAsync(l => l.LeadId == updated.LeadId);
        Assert.Equal(RealEstateLeadStatuses.Negotiation, lead.Status);
    }
}
