using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Integration;

/// <summary>
/// End-to-end checks of the actual JWT bearer pipeline wired up in Program.cs (auth scheme,
/// AdminOnly/AgentOrAdmin policies, [Authorize] on RealEstateProjectsController) - unlike the
/// controller/service unit tests, these go through real HTTP requests and real ASP.NET Core
/// authorization middleware, so an attribute typo or misconfigured policy would actually fail here.
/// </summary>
public class AuthIntegrationTests : IClassFixture<RealEstateApiFactory>
{
    private readonly RealEstateApiFactory _factory;

    public AuthIntegrationTests(RealEstateApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProject_ReturnsUnauthorized_WithoutToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/realestate/projects", new
        {
            projectCode = "PRJ-NOAUTH", projectName = "No Auth", projectType = "Residential",
            status = "Planning", country = "UAE", city = "Dubai", currency = "AED"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListProjects_ReturnsOk_WithoutToken()
    {
        // The public catalog stays anonymous - only writes require a token.
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/realestate/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LoginThenCreateProject_Succeeds_WithAgentToken()
    {
        await _factory.SeedUserAsync("agent-create-it@realestate.local", "Agent123!", RealEstateRoles.Agent);
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email = "agent-create-it@realestate.local", password = "Agent123!" });
        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var createResponse = await client.PostAsJsonAsync("/api/realestate/projects", new
        {
            projectCode = "PRJ-IT-CREATE", projectName = "IT Create", projectType = "Residential",
            status = "Planning", country = "UAE", city = "Dubai", currency = "AED"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    }

    [Fact]
    public async Task AgentToken_CannotApproveProject_ReturnsForbidden()
    {
        await _factory.SeedUserAsync("agent-approve-it@realestate.local", "Agent123!", RealEstateRoles.Agent);
        var projectId = await _factory.SeedProjectAsync("PRJ-IT-APPROVE-DENY");
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email = "agent-approve-it@realestate.local", password = "Agent123!" });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var approveResponse = await client.PostAsync($"/api/realestate/projects/{projectId}/approve", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, approveResponse.StatusCode);
    }

    [Fact]
    public async Task AdminToken_CanApproveProject_ReturnsOk()
    {
        await _factory.SeedUserAsync("admin-approve-it@realestate.local", "Admin123!", RealEstateRoles.Admin);
        var projectId = await _factory.SeedProjectAsync("PRJ-IT-APPROVE-ALLOW");
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin-approve-it@realestate.local", password = "Admin123!" });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var approveResponse = await client.PostAsync($"/api/realestate/projects/{projectId}/approve", content: null);

        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
    }
}
