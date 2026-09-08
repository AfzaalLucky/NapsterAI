using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateAnalyticsServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static async Task<Customer> SeedCustomerAsync(RealEstateDbContext db, string email)
    {
        var customer = new Customer { FullName = "Test Customer", Email = email };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    [Fact]
    public async Task GetLeadsSummaryAsync_ComputesConversionRateAndPipelineValue()
    {
        using var db = CreateContext();
        var customer = await SeedCustomerAsync(db, "a@example.com");

        db.Leads.AddRange(
            new Lead { CustomerId = customer.CustomerId, Status = RealEstateLeadStatuses.Won, Budget = 500_000m },
            new Lead { CustomerId = customer.CustomerId, Status = RealEstateLeadStatuses.Lost, Budget = 300_000m },
            new Lead { CustomerId = customer.CustomerId, Status = RealEstateLeadStatuses.New, Budget = 1_000_000m });
        await db.SaveChangesAsync();

        var service = new RealEstateAnalyticsService(db);
        var summary = await service.GetLeadsSummaryAsync(CancellationToken.None);

        Assert.Equal(3, summary.TotalLeads);
        Assert.Equal(50.0m, summary.ConversionRatePercent);
        Assert.Equal(1_000_000m, summary.PipelineValue);
    }

    [Fact]
    public async Task GetLeadsSummaryAsync_ConversionRateIsNull_WhenNoTerminalLeads()
    {
        using var db = CreateContext();
        var customer = await SeedCustomerAsync(db, "b@example.com");
        db.Leads.Add(new Lead { CustomerId = customer.CustomerId, Status = RealEstateLeadStatuses.New });
        await db.SaveChangesAsync();

        var service = new RealEstateAnalyticsService(db);
        var summary = await service.GetLeadsSummaryAsync(CancellationToken.None);

        Assert.Null(summary.ConversionRatePercent);
    }

    [Fact]
    public async Task GetInventorySummaryAsync_AggregatesByStatus()
    {
        using var db = CreateContext();
        db.Projects.Add(new Project
        {
            ProjectCode = "PRJ-100", ProjectName = "Test", ProjectType = "Residential",
            Status = "Ready", Country = "UAE", City = "Dubai", Currency = "AED"
        });
        await db.SaveChangesAsync();
        var projectId = (await db.Projects.FirstAsync()).ProjectId;

        db.Inventory.AddRange(
            new Inventory { ProjectId = projectId, UnitTypeId = 1, UnitNumber = "A-1", Status = RealEstateInventoryStatuses.Available, ListPrice = 1_000_000m },
            new Inventory { ProjectId = projectId, UnitTypeId = 1, UnitNumber = "A-2", Status = RealEstateInventoryStatuses.Sold, ListPrice = 2_000_000m });
        await db.SaveChangesAsync();

        var service = new RealEstateAnalyticsService(db);
        var summary = await service.GetInventorySummaryAsync(CancellationToken.None);

        Assert.Equal(1, summary.TotalProjects);
        Assert.Equal(2, summary.TotalUnits);
        Assert.Equal(3_000_000m, summary.TotalListValue);
        Assert.Equal(1, summary.ByStatus[RealEstateInventoryStatuses.Available]);
        Assert.Equal(1, summary.ByStatus[RealEstateInventoryStatuses.Sold]);
    }
}
