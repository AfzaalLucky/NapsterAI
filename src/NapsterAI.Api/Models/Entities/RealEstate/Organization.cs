namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>A real estate agency/brokerage that sales agents belong to.</summary>
public class Organization
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SalesAgent> SalesAgents { get; set; } = new List<SalesAgent>();
}
