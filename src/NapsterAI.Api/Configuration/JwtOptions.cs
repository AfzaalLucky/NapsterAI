namespace NapsterAI.Api.Configuration;

/// <summary>
/// Strongly typed settings bound from the "Jwt" configuration section, used to issue
/// and validate the Real Estate module's access/refresh tokens. SigningKey should never
/// be committed to source control - see README for how to supply it via user-secrets or
/// environment variables (same convention as Napster:ApiKey). If left empty, Program.cs
/// generates a random ephemeral key for that run so the app still starts locally, but
/// tokens issued before a restart stop validating afterwards.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "NapsterAI";
    public string Audience { get; set; } = "NapsterAI";

    /// <summary>HMAC-SHA256 signing key. Program.cs guarantees this is non-empty by the time it's read.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
