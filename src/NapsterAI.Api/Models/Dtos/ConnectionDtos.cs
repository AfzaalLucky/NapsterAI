using System.Text.Json;

namespace NapsterAI.Api.Models.Dtos;

/// <summary>
/// Body for POST /api/connections. Mirrors POST /public/connections - creates a
/// live (WebRTC/WebSocket) session token a client can use to talk to a companion.
/// "providerConfig" has a provider-specific shape not pinned down by the public
/// docs, so it's passed straight through as raw JSON - see
/// https://developers.napster.com/docs/api-reference/connections/post.
///
/// Note: despite not being documented at all for this endpoint, Napster's own
/// validation rejects connection creation unless providerConfig contains a
/// "voiceId" string (confirmed live: 400 "Voice id is required." without it,
/// success with e.g. providerConfig: { "voiceId": "en-US-JennyNeural" }).
/// NapsterService.CreateConnectionAsync checks for this client-side too.
/// </summary>
public class CreateConnectionRequestDto
{
    public string CompanionId { get; set; } = string.Empty;
    public JsonElement ProviderConfig { get; set; }
    public string? VideoPolicy { get; set; }
    public bool? DisableIdleTimeout { get; set; }
    public bool? UseWebSearch { get; set; }
    public IReadOnlyList<string>? Functions { get; set; }
    public string? KnowledgeBaseId { get; set; }
    public string? ExternalClientId { get; set; }
    public string? Language { get; set; }
    public string? InitialSpeech { get; set; }
}

/// <summary>
/// The client uses <see cref="Token"/> to establish the real-time connection
/// (WebRTC/WebSocket) with Napster directly - our API only brokers the token.
/// </summary>
public class ConnectionDto
{
    public string Token { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
}
