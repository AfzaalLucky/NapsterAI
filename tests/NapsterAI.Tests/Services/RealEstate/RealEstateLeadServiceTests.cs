using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateLeadServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static CreateLeadRequestDto ValidRequest() => new()
    {
        CustomerName = "Jane Buyer",
        CustomerEmail = "jane@example.com",
        CustomerPhone = "+971500000000",
        Source = RealEstateLeadSources.Website,
        Budget = 1_500_000m
    };

    [Fact]
    public async Task CreateAsync_Throws_WhenProjectIdDoesNotExist()
    {
        using var db = CreateContext();
        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);

        var request = ValidRequest();
        request.ProjectId = 999;

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_AndDefaultsStatusToNew()
    {
        using var db = CreateContext();
        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);

        var result = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.True(result.LeadId > 0);
        Assert.Equal(RealEstateLeadStatuses.New, result.Status);
        Assert.Equal("jane@example.com", result.Customer.Email);
    }

    [Fact]
    public async Task CreateAsync_ReusesExistingCustomer_WhenEmailAlreadyKnown()
    {
        using var db = CreateContext();
        db.Customers.Add(new Customer { FullName = "Jane Buyer", Email = "jane@example.com", Phone = "+971500000000" });
        await db.SaveChangesAsync();

        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);
        await service.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.Equal(1, await db.Customers.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_WhenStatusInvalid()
    {
        using var db = CreateContext();
        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);
        var lead = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.UpdateStatusAsync(lead.LeadId, new UpdateLeadStatusRequestDto { Status = "Bogus" }, null, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatus_AndRecordsActivity()
    {
        using var db = CreateContext();
        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);
        var lead = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        var updated = await service.UpdateStatusAsync(
            lead.LeadId, new UpdateLeadStatusRequestDto { Status = RealEstateLeadStatuses.Contacted }, actingUserId: null, CancellationToken.None);

        Assert.Equal(RealEstateLeadStatuses.Contacted, updated.Status);
        Assert.NotNull(updated.LastContactedDate);

        var activities = await service.ListActivitiesAsync(lead.LeadId, CancellationToken.None);
        Assert.Contains(activities, a => a.ActivityType == RealEstateLeadActivityTypes.StatusChange);
    }

    [Fact]
    public async Task AssignAsync_Throws_WhenSalesAgentDoesNotExist()
    {
        using var db = CreateContext();
        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);
        var lead = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.AssignAsync(lead.LeadId, new AssignLeadRequestDto { SalesAgentId = 999 }, null, CancellationToken.None));
    }

    [Fact]
    public async Task AssignAsync_SetsSalesAgent_AndRecordsActivity()
    {
        using var db = CreateContext();
        db.Organizations.Add(new Organization { OrganizationId = 1, Name = "Acme Realty" });
        db.SalesAgents.Add(new SalesAgent { SalesAgentId = 1, OrganizationId = 1, FullName = "Sam Agent" });
        await db.SaveChangesAsync();

        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);
        var lead = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        var assigned = await service.AssignAsync(lead.LeadId, new AssignLeadRequestDto { SalesAgentId = 1 }, null, CancellationToken.None);

        Assert.Equal(1, assigned.SalesAgentId);
        Assert.Equal("Sam Agent", assigned.SalesAgentName);
    }

    [Fact]
    public async Task AddActivityAsync_Throws_WhenActivityTypeInvalid()
    {
        using var db = CreateContext();
        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);
        var lead = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.AddActivityAsync(lead.LeadId, new CreateLeadActivityRequestDto { ActivityType = "Bogus" }, null, CancellationToken.None));
    }

    [Fact]
    public async Task ListAsync_FiltersByStatus()
    {
        using var db = CreateContext();
        var service = new RealEstateLeadService(db, NullLogger<RealEstateLeadService>.Instance);

        var lead1 = await service.CreateAsync(ValidRequest(), CancellationToken.None);
        var otherRequest = ValidRequest();
        otherRequest.CustomerEmail = "other@example.com";
        await service.CreateAsync(otherRequest, CancellationToken.None);

        await service.UpdateStatusAsync(lead1.LeadId, new UpdateLeadStatusRequestDto { Status = RealEstateLeadStatuses.Qualified }, null, CancellationToken.None);

        var result = await service.ListAsync(RealEstateLeadStatuses.Qualified, null, null, 0, 20, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(lead1.LeadId, result.Items[0].LeadId);
        Assert.Equal(2, result.TotalCount);
    }
}
