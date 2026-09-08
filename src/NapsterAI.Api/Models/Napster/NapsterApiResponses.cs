using System.Text.Json;

namespace NapsterAI.Api.Models.Napster;

// These classes mirror the raw JSON shape returned by the Napster Companion API
// (https://developers.napster.com/docs/api-reference). They are internal on
// purpose: controllers and callers should only ever see our own DTOs
// (Models/Dtos), never the third-party response shape.
//
// A handful of fields (providerSettings, mcp, tags, channels, ...) have rich,
// provider-specific shapes that aren't pinned down in the public docs, so we
// pass them through as raw JsonElement rather than guessing at a schema.

internal sealed class NapsterCompanion
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PreviewUrl { get; set; }
    public string? VideoLoopUrl { get; set; }
    public string? Ethnicity { get; set; }
    public string? Gender { get; set; }
    public string? Headline { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
    public string? Status { get; set; }
    public long? CreatedAt { get; set; }
    public string? ExternalClientId { get; set; }
    public List<string>? Versions { get; set; }
}

internal sealed class NapsterPagedResponse<T>
{
    public List<T>? Items { get; set; }
    public int FilteredCount { get; set; }
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
}

internal sealed class NapsterAgent
{
    public string? Id { get; set; }
    public string? CompanionId { get; set; }
    public string? Name { get; set; }
    public string? PreviewUrl { get; set; }
    public string? Language { get; set; }
    public string? VoiceId { get; set; }
    public List<string>? Functions { get; set; }
    public JsonElement? Mcp { get; set; }
    public List<string>? FaqCollections { get; set; }
    public string? KnowledgeBaseId { get; set; }
    public JsonElement? ProviderSettings { get; set; }
    public JsonElement? Tags { get; set; }
    public bool? DisableIdleTimeout { get; set; }
    public bool? UseWebSearch { get; set; }
    public JsonElement? Channels { get; set; }
    public long? Created { get; set; }
}

internal sealed class NapsterConnectionResponse
{
    public string? Token { get; set; }
    public NapsterConnectionRef? Connection { get; set; }
}

internal sealed class NapsterConnectionRef
{
    public string? Id { get; set; }
}

internal sealed class NapsterSessionCompanionRef
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PreviewUrl { get; set; }
    public string? UserId { get; set; }
}

internal sealed class NapsterSessionAgentRef
{
    public string? Name { get; set; }
}

internal sealed class NapsterSession
{
    public string? Id { get; set; }
    public string? CompanionId { get; set; }
    public string? KnowledgeBaseId { get; set; }
    public NapsterSessionCompanionRef? Companion { get; set; }
    public string? ExternalClientId { get; set; }
    public string? SessionType { get; set; }
    public string? Modality { get; set; }
    public string? Status { get; set; }
    public NapsterSessionAgentRef? Agent { get; set; }
    public string? CloseReason { get; set; }
    public double? Cost { get; set; }
    public long? CreatedAt { get; set; }
    public long? StartedAt { get; set; }
    public long? ClosedAt { get; set; }
}

internal sealed class NapsterKnowledgeBase
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Provider { get; set; }
    public int? ItemsCount { get; set; }
    public long? Created { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}

internal sealed class NapsterFaqCollection
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public long? Created { get; set; }
    public int? ItemsCount { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}

internal sealed class NapsterFaqItem
{
    public string? Id { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public long? CreatedAt { get; set; }
}
