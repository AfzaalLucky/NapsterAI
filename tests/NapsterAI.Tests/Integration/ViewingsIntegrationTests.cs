using System.Net;
using System.Net.Http.Json;
using NapsterAI.Api.Models.Dtos.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Integration;

/// <summary>
/// End-to-end checks of POST /api/realestate/viewings through the real ASP.NET Core pipeline -
/// this is the public "Book a viewing" form entrypoint, so it must work anonymously, and the
/// service transactionally upserts Customer + Lead + Viewing in one call (see
/// RealEstateViewingService.CreateAsync), which only a real DB round-trip actually exercises.
/// </summary>
public class ViewingsIntegrationTests : IClassFixture<RealEstateApiFactory>
{
    private readonly RealEstateApiFactory _factory;

    public ViewingsIntegrationTests(RealEstateApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateViewing_ReturnsCreated_WithoutToken()
    {
        var projectId = await _factory.SeedProjectAsync("PRJ-IT-VIEWING-OK");
        var inventoryId = await _factory.SeedInventoryAsync(projectId, "101");
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/realestate/viewings", new
        {
            customerName = "John Smith", customerEmail = "john.smith@example.com",
            inventoryId, scheduledDate = DateTime.UtcNow.AddDays(3)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var viewing = await response.Content.ReadFromJsonAsync<ViewingDto>();
        Assert.NotNull(viewing);
        Assert.True(viewing!.ViewingId > 0);
        Assert.Equal(inventoryId, viewing.InventoryId);
        Assert.Equal("Requested", viewing.Status);
    }

    [Fact]
    public async Task CreateViewing_ReturnsBadRequest_WhenInventoryIdUnknown()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/realestate/viewings", new
        {
            customerName = "John Smith", customerEmail = "john.smith@example.com",
            inventoryId = 999999, scheduledDate = DateTime.UtcNow.AddDays(3)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
