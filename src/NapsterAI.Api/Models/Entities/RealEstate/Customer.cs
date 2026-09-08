namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// A prospective buyer/tenant. Created/updated by an upsert-by-email (see
/// Services\RealEstate\CustomerUpsert.cs) whenever an Inquiry, Lead, or Viewing is created -
/// there is no dedicated public CRUD API for Customers themselves.
/// </summary>
public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;

    /// <summary>Always stored lower-cased; the natural key used for upsert matching.</summary>
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Nationality { get; set; }
    public string? PreferredLanguage { get; set; }

    /// <summary>Website / AI Assistant / Call / WalkIn - see <see cref="RealEstateLeadSources"/>.</summary>
    public string? Source { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public static class RealEstateLeadSources
{
    public const string Website = "Website";
    public const string AiAssistant = "AI Assistant";
    public const string Call = "Call";
    public const string WalkIn = "WalkIn";

    public static readonly string[] All = [Website, AiAssistant, Call, WalkIn];
}
