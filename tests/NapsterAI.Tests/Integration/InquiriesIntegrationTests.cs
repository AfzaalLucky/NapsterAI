using System.Net;
using System.Net.Http.Json;
using NapsterAI.Api.Models.Dtos.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Integration;

/// <summary>
/// End-to-end checks of POST /api/realestate/inquiries through the real ASP.NET Core pipeline
/// (routing, model binding, [AllowAnonymous], rate limiting, ExceptionHandlingMiddleware) rather
/// than mocked units - this is the public "Request info" form entrypoint, so it must genuinely
/// work without an auth token.
/// </summary>
public class InquiriesIntegrationTests : IClassFixture<RealEstateApiFactory>
{
    private readonly RealEstateApiFactory _factory;

    public InquiriesIntegrationTests(RealEstateApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateInquiry_ReturnsCreated_WithoutToken()
    {
        var projectId = await _factory.SeedProjectAsync("PRJ-IT-INQUIRY-OK");
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/realestate/inquiries", new
        {
            customerName = "Jane Doe", customerEmail = "jane.doe@example.com",
            projectId, channel = "Website", message = "Interested in a 2BR unit."
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var inquiry = await response.Content.ReadFromJsonAsync<InquiryDto>();
        Assert.NotNull(inquiry);
        Assert.True(inquiry!.InquiryId > 0);
        Assert.Equal(projectId, inquiry.ProjectId);
    }

    [Fact]
    public async Task CreateInquiry_ReturnsBadRequest_WhenChannelInvalid()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/realestate/inquiries", new
        {
            customerName = "Jane Doe", customerEmail = "jane.doe@example.com", channel = "Bogus"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
