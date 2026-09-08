using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NapsterAI.Api.Configuration;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Services;
using Xunit;

namespace NapsterAI.Tests.Services;

public class NapsterServiceTests
{
    private static readonly JsonElement EmptyJsonObject = JsonDocument.Parse("{}").RootElement;

    private static NapsterService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://companion-api.napster.com/public/")
        };

        var options = Options.Create(new NapsterOptions
        {
            BaseUrl = "https://companion-api.napster.com/public/",
            ApiKey = "test-api-key"
        });

        return new NapsterService(httpClient, options, NullLogger<NapsterService>.Instance);
    }

    [Theory]
    [InlineData(-1, 20)]
    [InlineData(0, 0)]
    [InlineData(0, 101)]
    public async Task BrowseCompanionsAsync_ThrowsInvalidRequestException_ForBadPaging(int pageIndex, int pageSize)
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.BrowseCompanionsAsync(null, null, null, pageIndex, pageSize, CancellationToken.None));
    }

    [Fact]
    public async Task BrowseCompanionsAsync_ThrowsInvalidRequestException_ForBadGender()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.BrowseCompanionsAsync(null, "robot", null, 0, 20, CancellationToken.None));
    }

    [Fact]
    public async Task BrowseCompanionsAsync_ReturnsMappedResults_OnSuccess()
    {
        const string json = """
        {
          "items": [
            { "id": "cmp.1", "firstName": "Alex", "lastName": "Rivers", "gender": "male", "status": "ready" }
          ],
          "filteredCount": 1,
          "totalCount": 42,
          "pageIndex": 0,
          "pageSize": 20
        }
        """;

        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, json));

        var result = await service.BrowseCompanionsAsync("alex", null, null, 0, 20, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Alex", result.Items[0].FirstName);
        Assert.Equal(42, result.TotalCount);
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsInvalidRequestException_WhenCompanionIdMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        var request = new CreateAgentRequestDto { CompanionId = "", Name = "My Agent", ProviderSettings = EmptyJsonObject };

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsInvalidRequestException_WhenProviderSettingsMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent" };

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsInvalidRequestException_WhenVoiceIdMissing()
    {
        // Marked optional in the public docs, but Napster's own validation rejects agent
        // creation without one - we check this client-side too, see NapsterService.CreateAgentAsync.
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", ProviderSettings = EmptyJsonObject };

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsInvalidRequestException_WhenNapsterReturns400()
    {
        const string json = """{ "code": "ValidationError", "description": "name is required" }""";
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.BadRequest, json));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", VoiceId = "voice.1", ProviderSettings = EmptyJsonObject };

        var ex = await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));

        Assert.Equal("name is required", ex.Message);
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsInvalidRequestException_WithMessageFromErrorArray()
    {
        // Napster's business-rule validation errors come back as a JSON array,
        // not a single object - e.g. [{ "code": "SomeRule", "description": "...", ... }]
        const string json = """[{ "code": "CompanionNotEligible", "description": "This companion cannot be used for agents.", "type": 11, "numericType": 11 }]""";
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.BadRequest, json));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", VoiceId = "voice.1", ProviderSettings = EmptyJsonObject };

        var ex = await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));

        Assert.Equal("This companion cannot be used for agents.", ex.Message);
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsInvalidRequestException_WithMessageFromAspNetValidationShape()
    {
        // Missing-field checks come back as standard ASP.NET Core validation ProblemDetails.
        const string json = """{ "errors": { "CompanionId": ["The CompanionId field is required."] }, "title": "One or more validation errors occurred.", "status": 400 }""";
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.BadRequest, json));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", VoiceId = "voice.1", ProviderSettings = EmptyJsonObject };

        var ex = await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));

        Assert.Equal("The CompanionId field is required.", ex.Message);
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsNapsterApiException_WithTitleFromPlainProblemDetails_WhenUnauthorized()
    {
        // 401s come back as plain ProblemDetails with only a title, no "errors"/"description".
        const string json = """{ "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2", "title": "Unauthorized", "status": 401 }""";
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.Unauthorized, json));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", VoiceId = "voice.1", ProviderSettings = EmptyJsonObject };

        var ex = await Assert.ThrowsAsync<NapsterApiException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.UpstreamStatusCode);
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsNapsterConflictException_When409Returned()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.Conflict, "{}"));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", VoiceId = "voice.1", ProviderSettings = EmptyJsonObject };

        await Assert.ThrowsAsync<NapsterConflictException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsNapsterApiException_WhenUnauthorized()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.Unauthorized, "{}"));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", VoiceId = "voice.1", ProviderSettings = EmptyJsonObject };

        var ex = await Assert.ThrowsAsync<NapsterApiException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.UpstreamStatusCode);
    }

    [Fact]
    public async Task CreateAgentAsync_ThrowsNapsterApiException_OnNetworkFailure()
    {
        var service = CreateService(new FakeHttpMessageHandler(new HttpRequestException("connection reset")));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", VoiceId = "voice.1", ProviderSettings = EmptyJsonObject };

        await Assert.ThrowsAsync<NapsterApiException>(
            () => service.CreateAgentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAgentAsync_ReturnsMappedAgent_OnSuccess()
    {
        const string json = """
        {
          "id": "agt.1",
          "companionId": "cmp.1",
          "name": "My Agent",
          "language": "en-US",
          "created": 1234567890
        }
        """;

        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, json));

        var request = new CreateAgentRequestDto { CompanionId = "cmp.1", Name = "My Agent", VoiceId = "voice.1", ProviderSettings = EmptyJsonObject };
        var agent = await service.CreateAgentAsync(request, CancellationToken.None);

        Assert.Equal("agt.1", agent.Id);
        Assert.Equal("My Agent", agent.Name);
        Assert.Equal(1234567890, agent.Created);
    }

    [Fact]
    public async Task CreateConnectionAsync_ThrowsInvalidRequestException_WhenProviderConfigMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        var request = new CreateConnectionRequestDto { CompanionId = "cmp.1" };

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateConnectionAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateConnectionAsync_ThrowsInvalidRequestException_WhenProviderConfigHasNoVoiceId()
    {
        // Not documented for this endpoint at all, but Napster's own validation rejects
        // connection creation unless providerConfig contains a "voiceId" string - see
        // NapsterService.CreateConnectionAsync.
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        var request = new CreateConnectionRequestDto { CompanionId = "cmp.1", ProviderConfig = EmptyJsonObject };

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateConnectionAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateConnectionAsync_ReturnsMappedConnection_OnSuccess()
    {
        const string json = """
        {
          "token": "tok_abc123",
          "connection": { "id": "conn.1" }
        }
        """;

        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, json));

        var providerConfigWithVoiceId = JsonDocument.Parse("""{ "voiceId": "en-US-JennyNeural" }""").RootElement;
        var request = new CreateConnectionRequestDto { CompanionId = "cmp.1", ProviderConfig = providerConfigWithVoiceId };
        var connection = await service.CreateConnectionAsync(request, CancellationToken.None);

        Assert.Equal("tok_abc123", connection.Token);
        Assert.Equal("conn.1", connection.ConnectionId);
    }

    [Fact]
    public async Task ListSessionsAsync_ThrowsInvalidRequestException_ForBadSessionType()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.ListSessionsAsync(null, null, "carrier-pigeon", null, 0, 20, CancellationToken.None));
    }

    [Fact]
    public async Task ListSessionsAsync_ReturnsMappedResults_OnSuccess()
    {
        const string json = """
        {
          "items": [
            { "id": "ses.1", "companionId": "cmp.1", "sessionType": "webrtc", "status": "closed", "cost": 0.42 }
          ],
          "filteredCount": 1,
          "totalCount": 1,
          "pageIndex": 0,
          "pageSize": 20
        }
        """;

        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, json));

        var result = await service.ListSessionsAsync(null, null, "webrtc", null, 0, 20, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("ses.1", result.Items[0].Id);
        Assert.Equal(0.42, result.Items[0].Cost);
    }

    [Fact]
    public async Task ListKnowledgeBasesAsync_ThrowsInvalidRequestException_ForBadProvider()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.ListKnowledgeBasesAsync("carrierPigeonAI", null, 0, 20, CancellationToken.None));
    }

    [Fact]
    public async Task ListKnowledgeBasesAsync_ReturnsMappedResults_OnSuccess()
    {
        const string json = """
        {
          "items": [
            { "id": "kb.1", "name": "Support Docs", "provider": "azureOpenAI", "itemsCount": 3, "created": 1700000000, "tags": {} }
          ],
          "filteredCount": 1,
          "totalCount": 1,
          "pageIndex": 0,
          "pageSize": 20
        }
        """;

        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, json));

        var result = await service.ListKnowledgeBasesAsync(null, null, 0, 20, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Support Docs", result.Items[0].Name);
        Assert.Equal("azureOpenAI", result.Items[0].Provider);
    }

    [Fact]
    public async Task CreateKnowledgeBaseAsync_ThrowsInvalidRequestException_WhenNameMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        var request = new CreateKnowledgeBaseRequestDto { Name = "" };

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateKnowledgeBaseAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateKnowledgeBaseAsync_ThrowsInvalidRequestException_WhenNapsterReturns400()
    {
        // Real Napster behavior, confirmed live: [{ "code": "name", "description": "'name' must not be empty.", ... }]
        const string json = """[{ "code": "name", "description": "'name' must not be empty.", "type": "validation", "numericType": 2 }]""";
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.BadRequest, json));

        var request = new CreateKnowledgeBaseRequestDto { Name = "Support Docs" };

        var ex = await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateKnowledgeBaseAsync(request, CancellationToken.None));

        Assert.Equal("'name' must not be empty.", ex.Message);
    }

    [Fact]
    public async Task CreateKnowledgeBaseAsync_ReturnsMappedKnowledgeBase_OnSuccess()
    {
        const string json = """
        { "id": "kb.1", "name": "Support Docs", "provider": "azureOpenAI", "itemsCount": 0, "created": 1700000000, "tags": {} }
        """;

        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, json));

        var request = new CreateKnowledgeBaseRequestDto { Name = "Support Docs" };
        var knowledgeBase = await service.CreateKnowledgeBaseAsync(request, CancellationToken.None);

        Assert.Equal("kb.1", knowledgeBase.Id);
        Assert.Equal("Support Docs", knowledgeBase.Name);
        Assert.Equal("azureOpenAI", knowledgeBase.Provider);
    }

    [Fact]
    public async Task CreateFaqCollectionAsync_ThrowsInvalidRequestException_WhenNameMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        var request = new CreateFaqCollectionRequestDto { Name = "" };

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateFaqCollectionAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateFaqCollectionAsync_ThrowsInvalidRequestException_WhenFaqItemMissingAnswer()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        var request = new CreateFaqCollectionRequestDto
        {
            Name = "Billing FAQs",
            Faqs = [new FaqItemRequestDto { Question = "How do refunds work?", Answer = "" }]
        };

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.CreateFaqCollectionAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateFaqCollectionAsync_ReturnsMappedFaqCollection_OnSuccess()
    {
        const string json = """{ "id": "faq.1", "name": "Billing FAQs", "created": 1700000000, "itemsCount": 1 }""";
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, json));

        var request = new CreateFaqCollectionRequestDto
        {
            Name = "Billing FAQs",
            Faqs = [new FaqItemRequestDto { Question = "How do refunds work?", Answer = "Within 30 days." }]
        };
        var faqCollection = await service.CreateFaqCollectionAsync(request, CancellationToken.None);

        Assert.Equal("faq.1", faqCollection.Id);
        Assert.Equal("Billing FAQs", faqCollection.Name);
        Assert.Equal(1, faqCollection.ItemsCount);
    }

    [Fact]
    public async Task ListFaqItemsAsync_ThrowsInvalidRequestException_WhenFaqCollectionIdMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.ListFaqItemsAsync(" ", 0, 20, CancellationToken.None));
    }

    [Fact]
    public async Task ListFaqItemsAsync_ReturnsMappedResults_OnSuccess()
    {
        const string json = """
        {
          "items": [
            { "id": "item.1", "question": "How do refunds work?", "answer": "Within 30 days.", "createdAt": 1700000000 }
          ],
          "filteredCount": 1,
          "totalCount": 1,
          "pageIndex": 0,
          "pageSize": 20
        }
        """;

        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, json));

        var result = await service.ListFaqItemsAsync("faq.1", 0, 20, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("How do refunds work?", result.Items[0].Question);
        Assert.Equal("Within 30 days.", result.Items[0].Answer);
    }
}
