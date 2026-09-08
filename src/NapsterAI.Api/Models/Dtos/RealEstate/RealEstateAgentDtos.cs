namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>
/// A real estate sales agent. Prefixed "RealEstate" even though the namespace/route already
/// disambiguates, to avoid an accidental "using"-driven collision with the existing
/// Napster AgentDto (an AI conversation agent config - a completely different concept).
/// </summary>
public class RealEstateAgentDto
{
    public int SalesAgentId { get; set; }
    public int OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsActive { get; set; }
}

public class CreateRealEstateAgentRequestDto
{
    public int OrganizationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public string? LicenseNumber { get; set; }
}

public class UpdateRealEstateAgentRequestDto : CreateRealEstateAgentRequestDto
{
    public bool IsActive { get; set; } = true;
}
