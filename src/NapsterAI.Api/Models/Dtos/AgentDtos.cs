using System.Text.Json;

namespace NapsterAI.Api.Models.Dtos;

/// <summary>
/// Body for POST /api/agents. Mirrors POST /public/agents.
/// "providerSettings" and "mcp" have provider-specific shapes that aren't
/// pinned down by the public docs, so they're passed straight through as
/// raw JSON rather than modeled field-by-field - see
/// https://developers.napster.com/docs/api-reference/agents/post for the shape.
/// </summary>
public class CreateAgentRequestDto
{
    public string CompanionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public JsonElement ProviderSettings { get; set; }
    public string? Language { get; set; }

    /// <summary>
    /// Marked optional in the public docs, but Napster's own validation rejects agent
    /// creation without one - <see cref="Services.NapsterService"/> enforces this client-side too.
    /// </summary>
    public string? VoiceId { get; set; }
    public IReadOnlyList<string>? Functions { get; set; }
    public JsonElement? Mcp { get; set; }
    public IReadOnlyList<string>? FaqCollections { get; set; }
    public string? KnowledgeBaseId { get; set; }
    public JsonElement? Tags { get; set; }
    public bool? DisableIdleTimeout { get; set; }
    public bool? UseWebSearch { get; set; }
}

public class AgentDto
{
    public string Id { get; set; } = string.Empty;
    public string CompanionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public string? Language { get; set; }
    public string? VoiceId { get; set; }
    public IReadOnlyList<string> Functions { get; set; } = [];
    public bool? DisableIdleTimeout { get; set; }
    public bool? UseWebSearch { get; set; }

    /// <summary>Epoch timestamp as returned by Napster; the docs don't specify seconds vs. milliseconds.</summary>
    public long? Created { get; set; }
}
