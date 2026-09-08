using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateAgentServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static async Task<int> SeedOrganizationAsync(RealEstateDbContext db)
    {
        var organization = new Organization { Name = "Acme Realty" };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();
        return organization.OrganizationId;
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenFullNameMissing()
    {
        using var db = CreateContext();
        var organizationId = await SeedOrganizationAsync(db);
        var service = new RealEstateAgentService(db);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(
            new CreateRealEstateAgentRequestDto { OrganizationId = organizationId, FullName = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenOrganizationDoesNotExist()
    {
        using var db = CreateContext();
        var service = new RealEstateAgentService(db);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(
            new CreateRealEstateAgentRequestDto { OrganizationId = 999, FullName = "Sam Agent" }, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_AndPopulatesOrganizationName()
    {
        using var db = CreateContext();
        var organizationId = await SeedOrganizationAsync(db);
        var service = new RealEstateAgentService(db);

        var result = await service.CreateAsync(
            new CreateRealEstateAgentRequestDto { OrganizationId = organizationId, FullName = "Sam Agent" }, CancellationToken.None);

        Assert.True(result.SalesAgentId > 0);
        Assert.Equal("Acme Realty", result.OrganizationName);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateAgentService(db);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task ListAsync_FiltersByOrganizationId()
    {
        using var db = CreateContext();
        var org1 = await SeedOrganizationAsync(db);
        db.Organizations.Add(new Organization { Name = "Other Realty" });
        await db.SaveChangesAsync();
        var org2 = await db.Organizations.Where(o => o.Name == "Other Realty").Select(o => o.OrganizationId).FirstAsync();

        var service = new RealEstateAgentService(db);
        await service.CreateAsync(new CreateRealEstateAgentRequestDto { OrganizationId = org1, FullName = "Agent One" }, CancellationToken.None);
        await service.CreateAsync(new CreateRealEstateAgentRequestDto { OrganizationId = org2, FullName = "Agent Two" }, CancellationToken.None);

        var result = await service.ListAsync(org1, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Agent One", result[0].FullName);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenReferencedByLead()
    {
        using var db = CreateContext();
        var organizationId = await SeedOrganizationAsync(db);
        var service = new RealEstateAgentService(db);
        var agent = await service.CreateAsync(
            new CreateRealEstateAgentRequestDto { OrganizationId = organizationId, FullName = "Sam Agent" }, CancellationToken.None);

        db.Customers.Add(new Customer { FullName = "Jane Buyer", Email = "jane@example.com" });
        await db.SaveChangesAsync();
        var customerId = (await db.Customers.FirstAsync()).CustomerId;
        db.Leads.Add(new Lead { CustomerId = customerId, SalesAgentId = agent.SalesAgentId });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<RealEstateConflictException>(
            () => service.DeleteAsync(agent.SalesAgentId, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_Succeeds_WhenNotReferenced()
    {
        using var db = CreateContext();
        var organizationId = await SeedOrganizationAsync(db);
        var service = new RealEstateAgentService(db);
        var agent = await service.CreateAsync(
            new CreateRealEstateAgentRequestDto { OrganizationId = organizationId, FullName = "Sam Agent" }, CancellationToken.None);

        await service.DeleteAsync(agent.SalesAgentId, CancellationToken.None);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(agent.SalesAgentId, CancellationToken.None));
    }
}
