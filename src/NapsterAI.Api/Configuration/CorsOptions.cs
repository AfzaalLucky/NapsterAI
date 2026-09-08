namespace NapsterAI.Api.Configuration;

/// <summary>
/// Strongly typed settings bound from the "Cors" configuration section - the dev/prod
/// origins allowed to call this API cross-origin (the Playground app, the new Real Estate
/// frontend, and any deployed origins). Replaces the previously hardcoded origin list.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}
