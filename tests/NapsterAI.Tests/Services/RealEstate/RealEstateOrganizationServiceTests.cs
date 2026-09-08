using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateOrganizationServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static CreateOrganizationRequestDto ValidRequest() => new() { Name = "Acme Realty", Email = "info@acme.example" };

    [Fact]
    public async Task CreateAsync_Throws_WhenNameMissing()
    {
        using var db = CreateContext();
        var service = new RealEstateOrganizationService(db);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateAsync(new CreateOrganizationRequestDto { Name = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_AndDefaultsIsActiveTrue()
    {
        using var db = CreateContext();
        var service = new RealEstateOrganizationService(db);

        var result = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.True(result.OrganizationId > 0);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateOrganizationService(db);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        using var db = CreateContext();
        var service = new RealEstateOrganizationService(db);
        var created = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        var updated = await service.UpdateAsync(
            created.OrganizationId,
            new UpdateOrganizationRequestDto { Name = "Acme Realty Group", Email = created.Email, IsActive = false },
            CancellationToken.None);

        Assert.Equal("Acme Realty Group", updated.Name);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenSalesAgentsStillReference()
    {
        using var db = CreateContext();
        var service = new RealEstateOrganizationService(db);
        var created = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        db.SalesAgents.Add(new SalesAgent { OrganizationId = created.OrganizationId, FullName = "Sam Agent" });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<RealEstateConflictException>(
            () => service.DeleteAsync(created.OrganizationId, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_Succeeds_WhenNoSalesAgentsReference()
    {
        using var db = CreateContext();
        var service = new RealEstateOrganizationService(db);
        var created = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        await service.DeleteAsync(created.OrganizationId, CancellationToken.None);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(created.OrganizationId, CancellationToken.None));
    }
}
