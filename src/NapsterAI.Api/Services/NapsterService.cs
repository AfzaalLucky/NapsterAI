using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NapsterAI.Api.Configuration;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Napster;

namespace NapsterAI.Api.Services;

/// <summary>
/// Talks to the Napster Companion API (https://developers.napster.com/docs/api-reference)
/// over HttpClient and maps the results onto our own DTOs. All failure modes
/// (network errors, non-success status codes, malformed JSON) are normalized
/// into the exception types in NapsterAI.Api.Exceptions so callers only have
/// to handle one family of errors.
/// </summary>
public class NapsterService : INapsterService
{
    private static readonly string[] ValidGenders = ["male", "female", "nonBinary"];
    private static readonly string[] ValidSessionTypes = ["voip", "sip", "webrtc", "websocket", "kiosk"];
    private static readonly string[] ValidKnowledgeBaseProviders = ["azureOpenAI", "gemini", "openAI", "humain", "microsoftFoundry"];

    // Not documented, but Napster's actual validation rejects an externalClientId that
    // doesn't match this shape (observed: 400 "Invalid external client ID. Allowed characters
    // are letters, digits, hyphens (-), and underscores (_), with a maximum of 32 characters."
    // from POST /connections).
    private static readonly Regex ExternalClientIdPattern = new("^[A-Za-z0-9_-]{1,32}$", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly NapsterOptions _options;
    private readonly ILogger<NapsterService> _logger;

    public NapsterService(HttpClient httpClient, IOptions<NapsterOptions> options, ILogger<NapsterService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning(
                "Napster API key is not configured. Requests to the Napster API will fail with 401. " +
                "Set it via 'dotnet user-secrets set Napster:ApiKey <key>' or the Napster__ApiKey environment variable.");
        }
    }

    public async Task<PagedResultDto<CompanionDto>> BrowseCompanionsAsync(
        string? search, string? gender, string? ethnicity, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        if (gender is not null && !ValidGenders.Contains(gender))
        {
            throw new InvalidRequestException($"gender must be one of: {string.Join(", ", ValidGenders)}.");
        }

        var query = new List<string> { $"pageIndex={pageIndex}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(gender)) query.Add($"gender={Uri.EscapeDataString(gender)}");
        if (!string.IsNullOrWhiteSpace(ethnicity)) query.Add($"ethnicity={Uri.EscapeDataString(ethnicity)}");

        var path = $"companions/napster-stock?{string.Join('&', query)}";

        _logger.LogInformation("Browsing Napster stock companions (search={Search})", search);

        var response = await SendAsync(HttpMethod.Get, path, body: null, cancellationToken);
        var envelope = await ReadAsync<NapsterPagedResponse<NapsterCompanion>>(response, cancellationToken);

        return new PagedResultDto<CompanionDto>
        {
            Items = (envelope.Items ?? []).Select(MapCompanion).ToList(),
            TotalCount = envelope.TotalCount,
            FilteredCount = envelope.FilteredCount,
            PageIndex = envelope.PageIndex,
            PageSize = envelope.PageSize
        };
    }

    public async Task<AgentDto> CreateAgentAsync(CreateAgentRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CompanionId))
        {
            throw new InvalidRequestException("companionId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidRequestException("name is required.");
        }

        if (request.ProviderSettings.ValueKind is JsonValueKind.Undefined)
        {
            throw new InvalidRequestException("providerSettings is required.");
        }

        // The public docs mark voiceId as optional, but Napster's actual validation rejects
        // agent creation without one (observed: 400 "Voice id is required." from POST /agents).
        // Checking it here fails fast with a clear message instead of round-tripping to Napster.
        if (string.IsNullOrWhiteSpace(request.VoiceId))
        {
            throw new InvalidRequestException("voiceId is required (Napster rejects agent creation without one, despite the public docs marking it optional).");
        }

        _logger.LogInformation("Creating Napster agent {Name} for companion {CompanionId}", request.Name, request.CompanionId);

        var response = await SendAsync(HttpMethod.Post, "agents", request, cancellationToken);
        var agent = await ReadAsync<NapsterAgent>(response, cancellationToken);

        return MapAgent(agent);
    }

    public async Task<ConnectionDto> CreateConnectionAsync(CreateConnectionRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CompanionId))
        {
            throw new InvalidRequestException("companionId is required.");
        }

        if (request.ProviderConfig.ValueKind is JsonValueKind.Undefined)
        {
            throw new InvalidRequestException("providerConfig is required.");
        }

        // Not documented at all for this endpoint, but Napster's actual validation rejects
        // connection creation unless providerConfig contains a "voiceId" string (observed:
        // 400 "Voice id is required." from POST /connections without it). Checking it here
        // fails fast with a clear message instead of round-tripping to Napster.
        var hasVoiceId = request.ProviderConfig.ValueKind is JsonValueKind.Object
            && request.ProviderConfig.TryGetProperty("voiceId", out var voiceIdElement)
            && voiceIdElement.ValueKind is JsonValueKind.String
            && !string.IsNullOrWhiteSpace(voiceIdElement.GetString());

        if (!hasVoiceId)
        {
            throw new InvalidRequestException(
                "providerConfig.voiceId is required (Napster rejects connection creation without one, despite it not being documented for this endpoint).");
        }

        if (request.ExternalClientId is not null && !ExternalClientIdPattern.IsMatch(request.ExternalClientId))
        {
            throw new InvalidRequestException(
                "externalClientId must contain only letters, digits, hyphens (-), and underscores (_), with a maximum of 32 characters.");
        }

        _logger.LogInformation("Creating Napster connection for companion {CompanionId}", request.CompanionId);

        var response = await SendAsync(HttpMethod.Post, "connections", request, cancellationToken);
        var connection = await ReadAsync<NapsterConnectionResponse>(response, cancellationToken);

        if (string.IsNullOrEmpty(connection.Token) || connection.Connection?.Id is null)
        {
            throw new NapsterApiException("Napster API returned an incomplete connection response.");
        }

        return new ConnectionDto
        {
            Token = connection.Token,
            ConnectionId = connection.Connection.Id
        };
    }

    public async Task<PagedResultDto<SessionDto>> ListSessionsAsync(
        string? companionId, string? externalClientId, string? sessionType, string? search,
        int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        if (sessionType is not null && !ValidSessionTypes.Contains(sessionType))
        {
            throw new InvalidRequestException($"sessionType must be one of: {string.Join(", ", ValidSessionTypes)}.");
        }

        var query = new List<string> { $"pageIndex={pageIndex}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(companionId)) query.Add($"companionId={Uri.EscapeDataString(companionId)}");
        if (!string.IsNullOrWhiteSpace(externalClientId)) query.Add($"externalClientId={Uri.EscapeDataString(externalClientId)}");
        if (!string.IsNullOrWhiteSpace(sessionType)) query.Add($"sessionType={Uri.EscapeDataString(sessionType)}");
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");

        var path = $"sessions?{string.Join('&', query)}";

        _logger.LogInformation("Listing Napster sessions (companionId={CompanionId})", companionId);

        var response = await SendAsync(HttpMethod.Get, path, body: null, cancellationToken);
        var envelope = await ReadAsync<NapsterPagedResponse<NapsterSession>>(response, cancellationToken);

        return new PagedResultDto<SessionDto>
        {
            Items = (envelope.Items ?? []).Select(MapSession).ToList(),
            TotalCount = envelope.TotalCount,
            FilteredCount = envelope.FilteredCount,
            PageIndex = envelope.PageIndex,
            PageSize = envelope.PageSize
        };
    }

    public async Task<PagedResultDto<KnowledgeBaseDto>> ListKnowledgeBasesAsync(
        string? provider, string? search, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        if (provider is not null && !ValidKnowledgeBaseProviders.Contains(provider))
        {
            throw new InvalidRequestException($"provider must be one of: {string.Join(", ", ValidKnowledgeBaseProviders)}.");
        }

        var query = new List<string> { $"pageIndex={pageIndex}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(provider)) query.Add($"provider={Uri.EscapeDataString(provider)}");
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");

        var path = $"knowledge-bases?{string.Join('&', query)}";

        _logger.LogInformation("Listing Napster knowledge bases (search={Search})", search);

        var response = await SendAsync(HttpMethod.Get, path, body: null, cancellationToken);
        var envelope = await ReadAsync<NapsterPagedResponse<NapsterKnowledgeBase>>(response, cancellationToken);

        return new PagedResultDto<KnowledgeBaseDto>
        {
            Items = (envelope.Items ?? []).Select(MapKnowledgeBase).ToList(),
            TotalCount = envelope.TotalCount,
            FilteredCount = envelope.FilteredCount,
            PageIndex = envelope.PageIndex,
            PageSize = envelope.PageSize
        };
    }

    public async Task<KnowledgeBaseDto> CreateKnowledgeBaseAsync(CreateKnowledgeBaseRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidRequestException("name is required.");
        }

        if (request.Provider is not null && !ValidKnowledgeBaseProviders.Contains(request.Provider))
        {
            throw new InvalidRequestException($"provider must be one of: {string.Join(", ", ValidKnowledgeBaseProviders)}.");
        }

        _logger.LogInformation("Creating Napster knowledge base {Name}", request.Name);

        var response = await SendAsync(HttpMethod.Post, "knowledge-bases", request, cancellationToken);
        var knowledgeBase = await ReadAsync<NapsterKnowledgeBase>(response, cancellationToken);

        return MapKnowledgeBase(knowledgeBase);
    }

    public async Task<PagedResultDto<FaqCollectionDto>> ListFaqCollectionsAsync(
        string? search, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        var query = new List<string> { $"pageIndex={pageIndex}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");

        var path = $"faqs?{string.Join('&', query)}";

        _logger.LogInformation("Listing Napster FAQ collections (search={Search})", search);

        var response = await SendAsync(HttpMethod.Get, path, body: null, cancellationToken);
        var envelope = await ReadAsync<NapsterPagedResponse<NapsterFaqCollection>>(response, cancellationToken);

        return new PagedResultDto<FaqCollectionDto>
        {
            Items = (envelope.Items ?? []).Select(MapFaqCollection).ToList(),
            TotalCount = envelope.TotalCount,
            FilteredCount = envelope.FilteredCount,
            PageIndex = envelope.PageIndex,
            PageSize = envelope.PageSize
        };
    }

    public async Task<FaqCollectionDto> CreateFaqCollectionAsync(CreateFaqCollectionRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidRequestException("name is required.");
        }

        if (request.Faqs is not null)
        {
            foreach (var item in request.Faqs)
            {
                if (string.IsNullOrWhiteSpace(item.Question) || string.IsNullOrWhiteSpace(item.Answer))
                {
                    throw new InvalidRequestException("each entry in faqs must have a non-empty question and answer.");
                }
            }
        }

        _logger.LogInformation("Creating Napster FAQ collection {Name}", request.Name);

        var response = await SendAsync(HttpMethod.Post, "faqs", request, cancellationToken);
        var faqCollection = await ReadAsync<NapsterFaqCollection>(response, cancellationToken);

        return MapFaqCollection(faqCollection);
    }

    public async Task<PagedResultDto<FaqItemDto>> ListFaqItemsAsync(
        string faqCollectionId, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        EnsureId(faqCollectionId, nameof(faqCollectionId));
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        var path = $"faqs/{Uri.EscapeDataString(faqCollectionId)}/items?pageIndex={pageIndex}&pageSize={pageSize}";

        _logger.LogInformation("Listing items for Napster FAQ collection {FaqCollectionId}", faqCollectionId);

        var response = await SendAsync(HttpMethod.Get, path, body: null, cancellationToken);
        var envelope = await ReadAsync<NapsterPagedResponse<NapsterFaqItem>>(response, cancellationToken);

        return new PagedResultDto<FaqItemDto>
        {
            Items = (envelope.Items ?? []).Select(MapFaqItem).ToList(),
            TotalCount = envelope.TotalCount,
            FilteredCount = envelope.FilteredCount,
            PageIndex = envelope.PageIndex,
            PageSize = envelope.PageSize
        };
    }

    private static void EnsureId(string id, string paramName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidRequestException($"{paramName} must not be empty.");
        }
    }

    private static void ValidatePaging(int pageIndex, int pageSize, int maxPageSize)
    {
        if (pageIndex < 0)
        {
            throw new InvalidRequestException("pageIndex must be 0 or greater.");
        }

        if (pageSize is < 1 || pageSize > maxPageSize)
        {
            throw new InvalidRequestException($"pageSize must be between 1 and {maxPageSize}.");
        }
    }

    /// <summary>
    /// Sends the request and translates any network-level failure (timeout, DNS,
    /// connection refused, etc.) into a NapsterApiException. Does not throw for
    /// non-success HTTP status codes - callers decide via <see cref="ReadAsync{T}"/>.
    /// </summary>
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string requestUri, object? body, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            using var requestMessage = new HttpRequestMessage(method, requestUri);
            if (body is not null)
            {
                requestMessage.Content = JsonContent.Create(body, options: JsonOptions);
            }

            response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The HttpClient's own timeout fired (distinct from the caller cancelling).
            _logger.LogError("Timed out calling Napster API at {RequestUri}", requestUri);
            throw new NapsterApiException("Timed out while calling the Napster API. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling Napster API at {RequestUri}", requestUri);
            throw new NapsterApiException("Could not reach the Napster API. Please try again later.", innerException: ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, cancellationToken);
        }

        return response;
    }

    private async Task HandleErrorResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusCode = response.StatusCode;
        var description = await ExtractErrorDescriptionAsync(response, cancellationToken);

        _logger.LogWarning("Napster API returned {StatusCode}: {Description}", (int)statusCode, description ?? "(no details)");

        switch (statusCode)
        {
            case HttpStatusCode.BadRequest:
                throw new InvalidRequestException(description ?? "Napster rejected the request as invalid.");

            case HttpStatusCode.NotFound:
                throw new NapsterResourceNotFoundException(description ?? "The requested resource was not found.");

            case HttpStatusCode.Conflict:
                throw new NapsterConflictException(description ?? "The request conflicts with existing state.");

            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new NapsterApiException(
                    "The Napster API rejected our credentials. Check that Napster:ApiKey is configured correctly.",
                    statusCode);

            case HttpStatusCode.TooManyRequests:
                throw new NapsterApiException("Napster API rate limit exceeded. Please slow down and try again shortly.", statusCode);

            default:
                throw new NapsterApiException(
                    description is not null
                        ? $"Napster API request failed ({(int)statusCode}): {description}"
                        : $"Napster API request failed with status code {(int)statusCode}.",
                    statusCode);
        }
    }

    /// <summary>
    /// Napster's error responses don't follow one consistent shape - observed so far:
    ///  - business-rule validation: a JSON array of { code, description, type, numericType } objects
    ///  - ASP.NET Core model validation: { errors: { "Field": ["message", ...] }, title, status, ... }
    ///  - everything else (401, unhandled 5xx, ...): plain ProblemDetails, { title, status, ... }
    /// This walks the raw JSON defensively rather than binding to a single fixed model, since a
    /// wrong assumption here means callers only ever see "request failed" instead of the real reason.
    /// </summary>
    private static async Task<string?> ExtractErrorDescriptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string body;

        try
        {
            body = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                var messages = root.EnumerateArray()
                    .Select(ExtractSingleErrorMessage)
                    .Where(message => message is not null)
                    .ToList();

                return messages.Count > 0 ? string.Join(" ", messages) : null;
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
            {
                var messages = errorsElement.EnumerateObject()
                    .SelectMany(field => field.Value.ValueKind == JsonValueKind.Array
                        ? field.Value.EnumerateArray().Select(m => m.GetString())
                        : [])
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .ToList();

                if (messages.Count > 0)
                {
                    return string.Join(" ", messages);
                }
            }

            var singleMessage = ExtractSingleErrorMessage(root);
            if (singleMessage is not null)
            {
                return singleMessage;
            }

            if (root.TryGetProperty("detail", out var detailElement) && detailElement.ValueKind == JsonValueKind.String)
            {
                return detailElement.GetString();
            }

            if (root.TryGetProperty("title", out var titleElement) && titleElement.ValueKind == JsonValueKind.String)
            {
                return titleElement.GetString();
            }

            return null;
        }
        catch (JsonException)
        {
            // Not JSON at all - fall back to the status code.
            return null;
        }
    }

    private static string? ExtractSingleErrorMessage(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (element.TryGetProperty("description", out var descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String)
        {
            return descriptionElement.GetString();
        }

        if (element.TryGetProperty("code", out var codeElement) && codeElement.ValueKind == JsonValueKind.String)
        {
            return codeElement.GetString();
        }

        return null;
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);

            if (result is null)
            {
                throw new NapsterApiException("Napster API returned an empty response.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Napster API response as {Type}", typeof(T).Name);
            throw new NapsterApiException("Received an unexpected response format from the Napster API.", innerException: ex);
        }
    }

    private static CompanionDto MapCompanion(NapsterCompanion companion) => new()
    {
        Id = companion.Id ?? string.Empty,
        FirstName = companion.FirstName ?? string.Empty,
        LastName = companion.LastName ?? string.Empty,
        PreviewUrl = companion.PreviewUrl,
        VideoLoopUrl = companion.VideoLoopUrl,
        Ethnicity = companion.Ethnicity,
        Gender = companion.Gender,
        Headline = companion.Headline,
        Tags = companion.Tags ?? new Dictionary<string, string>(),
        Status = companion.Status
    };

    private static AgentDto MapAgent(NapsterAgent agent) => new()
    {
        Id = agent.Id ?? string.Empty,
        CompanionId = agent.CompanionId ?? string.Empty,
        Name = agent.Name ?? string.Empty,
        PreviewUrl = agent.PreviewUrl,
        Language = agent.Language,
        VoiceId = agent.VoiceId,
        Functions = agent.Functions ?? [],
        DisableIdleTimeout = agent.DisableIdleTimeout,
        UseWebSearch = agent.UseWebSearch,
        Created = agent.Created
    };

    private static SessionDto MapSession(NapsterSession session) => new()
    {
        Id = session.Id ?? string.Empty,
        CompanionId = session.CompanionId,
        CompanionFirstName = session.Companion?.FirstName,
        CompanionLastName = session.Companion?.LastName,
        ExternalClientId = session.ExternalClientId,
        SessionType = session.SessionType,
        Modality = session.Modality,
        Status = session.Status,
        AgentName = session.Agent?.Name,
        CloseReason = session.CloseReason,
        Cost = session.Cost,
        CreatedAt = session.CreatedAt,
        StartedAt = session.StartedAt,
        ClosedAt = session.ClosedAt
    };

    private static KnowledgeBaseDto MapKnowledgeBase(NapsterKnowledgeBase knowledgeBase) => new()
    {
        Id = knowledgeBase.Id ?? string.Empty,
        Name = knowledgeBase.Name ?? string.Empty,
        Provider = knowledgeBase.Provider,
        ItemsCount = knowledgeBase.ItemsCount,
        Tags = knowledgeBase.Tags ?? new Dictionary<string, string>(),
        Created = knowledgeBase.Created
    };

    private static FaqCollectionDto MapFaqCollection(NapsterFaqCollection faqCollection) => new()
    {
        Id = faqCollection.Id ?? string.Empty,
        Name = faqCollection.Name ?? string.Empty,
        ItemsCount = faqCollection.ItemsCount,
        Created = faqCollection.Created
    };

    private static FaqItemDto MapFaqItem(NapsterFaqItem faqItem) => new()
    {
        Id = faqItem.Id ?? string.Empty,
        Question = faqItem.Question ?? string.Empty,
        Answer = faqItem.Answer ?? string.Empty,
        CreatedAt = faqItem.CreatedAt
    };
}
