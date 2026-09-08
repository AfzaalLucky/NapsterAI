using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateProjectServiceTests
{
    // Microsoft.EntityFrameworkCore.InMemory stands in for SQL Server here - it doesn't
    // enforce our FK/unique constraints, but the service's own validate-before-write
    // checks (mirroring Services\NapsterService.cs's idiom) run in C#, so this exercises
    // the same code paths a real database would.
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static CreateProjectRequestDto ValidRequest(string code = "PRJ-100") => new()
    {
        ProjectCode = code,
        ProjectName = "Test Project",
        ProjectType = "Residential",
        Status = "Planning",
        Country = "UAE",
        City = "Dubai",
        Currency = "AED"
    };

    [Fact]
    public async Task CreateAsync_Throws_WhenProjectTypeInvalid()
    {
        using var db = CreateContext();
        var service = new RealEstateProjectService(db, NullLogger<RealEstateProjectService>.Instance);

        var request = ValidRequest();
        request.ProjectType = "Spaceship";

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenStatusInvalid()
    {
        using var db = CreateContext();
        var service = new RealEstateProjectService(db, NullLogger<RealEstateProjectService>.Instance);

        var request = ValidRequest();
        request.Status = "Bogus";

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_AndDefaultsToApprovalStatusDraft()
    {
        using var db = CreateContext();
        var service = new RealEstateProjectService(db, NullLogger<RealEstateProjectService>.Instance);

        var result = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.True(result.ProjectId > 0);
        Assert.Equal(RealEstateApprovalStatuses.Draft, result.ApprovalStatus);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenProjectCodeAlreadyExists()
    {
        using var db = CreateContext();
        var service = new RealEstateProjectService(db, NullLogger<RealEstateProjectService>.Instance);

        await service.CreateAsync(ValidRequest("PRJ-DUP"), CancellationToken.None);

        await Assert.ThrowsAsync<RealEstateConflictException>(
            () => service.CreateAsync(ValidRequest("PRJ-DUP"), CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateProjectService(db, NullLogger<RealEstateProjectService>.Instance);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task ApproveAsync_SetsApprovalStatusToApproved()
    {
        using var db = CreateContext();
        var service = new RealEstateProjectService(db, NullLogger<RealEstateProjectService>.Instance);

        var created = await service.CreateAsync(ValidRequest(), CancellationToken.None);
        var approved = await service.ApproveAsync(created.ProjectId, CancellationToken.None);

        Assert.Equal(RealEstateApprovalStatuses.Approved, approved.ApprovalStatus);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletes_SoProjectNoLongerAppearsInListOrGet()
    {
        using var db = CreateContext();
        var service = new RealEstateProjectService(db, NullLogger<RealEstateProjectService>.Instance);

        var created = await service.CreateAsync(ValidRequest(), CancellationToken.None);
        await service.DeleteAsync(created.ProjectId, CancellationToken.None);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(created.ProjectId, CancellationToken.None));

        var list = await service.ListAsync(null, null, null, null, null, null, null, 0, 20, CancellationToken.None);
        Assert.DoesNotContain(list.Items, p => p.ProjectId == created.ProjectId);
    }

    [Fact]
    public async Task ListAsync_FiltersByCityAndIsFeatured()
    {
        using var db = CreateContext();
        var service = new RealEstateProjectService(db, NullLogger<RealEstateProjectService>.Instance);

        var dubaiFeatured = ValidRequest("PRJ-A");
        dubaiFeatured.City = "Dubai";
        dubaiFeatured.IsFeatured = true;

        var dubaiNotFeatured = ValidRequest("PRJ-B");
        dubaiNotFeatured.City = "Dubai";
        dubaiNotFeatured.IsFeatured = false;

        var otherCity = ValidRequest("PRJ-C");
        otherCity.City = "Islamabad";
        otherCity.IsFeatured = true;

        await service.CreateAsync(dubaiFeatured, CancellationToken.None);
        await service.CreateAsync(dubaiNotFeatured, CancellationToken.None);
        await service.CreateAsync(otherCity, CancellationToken.None);

        var result = await service.ListAsync("Dubai", null, null, true, null, null, null, 0, 20, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("PRJ-A", result.Items[0].ProjectCode);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.FilteredCount);
    }
}
