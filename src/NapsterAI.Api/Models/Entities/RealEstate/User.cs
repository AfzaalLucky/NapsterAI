namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// An authenticated account for the Real Estate module (admin or sales agent login).
/// Net-new - no auth or Users table existed anywhere in this repo before Phase 3.
/// </summary>
public class User
{
    public int UserId { get; set; }

    /// <summary>Always stored lower-cased for case-insensitive lookup.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>PBKDF2 salt+hash, see Services\RealEstate\PasswordHasher.cs. Never plaintext.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Admin or Agent - see <see cref="RealEstateRoles"/>.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Optional link to this account's sales-agent profile (Agent-role users only). Null for Admin accounts.</summary>
    public int? SalesAgentId { get; set; }
    public SalesAgent? SalesAgent { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Opaque random token; null until first login/refresh. Rotated on every use.</summary>
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
}

public static class RealEstateRoles
{
    public const string Admin = "Admin";
    public const string Agent = "Agent";

    public static readonly string[] All = [Admin, Agent];
}
