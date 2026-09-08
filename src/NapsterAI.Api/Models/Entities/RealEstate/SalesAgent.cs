namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// A real estate sales agent - distinct from Napster's "Agent" concept (an AI conversation
/// agent config, see AgentsController/AgentDto). A login account (User) optionally links to
/// one of these via User.SalesAgentId; this table itself has no back-reference to Users.
/// </summary>
public class SalesAgent
{
    public int SalesAgentId { get; set; }

    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
