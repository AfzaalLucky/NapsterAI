namespace NapsterAI.Api.Models.Dtos;

/// <summary>
/// Summary of a past conversation session, as returned by GET /public/sessions.
/// </summary>
public class SessionDto
{
    public string Id { get; set; } = string.Empty;
    public string? CompanionId { get; set; }
    public string? CompanionFirstName { get; set; }
    public string? CompanionLastName { get; set; }
    public string? ExternalClientId { get; set; }
    public string? SessionType { get; set; }
    public string? Modality { get; set; }
    public string? Status { get; set; }
    public string? AgentName { get; set; }
    public string? CloseReason { get; set; }
    public double? Cost { get; set; }

    /// <summary>Epoch timestamps as returned by Napster; the docs don't specify seconds vs. milliseconds.</summary>
    public long? CreatedAt { get; set; }
    public long? StartedAt { get; set; }
    public long? ClosedAt { get; set; }
}
