namespace NapsterAI.Api.Configuration;

/// <summary>
/// Strongly typed settings bound from the "Napster" configuration section.
/// The API key should never be committed to source control - see README for
/// how to supply it via user-secrets or environment variables.
/// </summary>
public class NapsterOptions
{
    public const string SectionName = "Napster";

    /// <summary>
    /// Napster's Companion/Agent platform API. Every path is under "/public".
    /// See https://developers.napster.com/docs/introduction/quickstart
    /// </summary>
    public string BaseUrl { get; set; } = "https://companion-api.napster.com/public/";

    /// <summary>
    /// Sent as the "X-Api-Key" header on every request. Server-side secret only.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional "X-API-Version" header some endpoints accept. Leave empty to omit it
    /// and let Napster use its default/current version.
    /// </summary>
    public string? ApiVersion { get; set; }

    public int TimeoutSeconds { get; set; } = 10;
}
