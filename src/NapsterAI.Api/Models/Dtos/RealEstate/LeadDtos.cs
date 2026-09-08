namespace NapsterAI.Api.Models.Dtos.RealEstate;

public class LeadDto
{
    public int LeadId { get; set; }
    public CustomerDto Customer { get; set; } = new();
    public int? ProjectId { get; set; }
    public int? InventoryId { get; set; }
    public int? SalesAgentId { get; set; }
    public string? SalesAgentName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Source { get; set; }
    public decimal? Budget { get; set; }
    public string? RequirementsNotes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastContactedDate { get; set; }
}

/// <summary>Direct lead creation (e.g. an agent logging a phone-in lead), distinct from converting an Inquiry.</summary>
public class CreateLeadRequestDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public int? ProjectId { get; set; }
    public int? InventoryId { get; set; }
    public string? Source { get; set; }
    public decimal? Budget { get; set; }
    public string? RequirementsNotes { get; set; }
}

public class UpdateLeadStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
}

public class AssignLeadRequestDto
{
    public int SalesAgentId { get; set; }
}

public class CreateLeadActivityRequestDto
{
    /// <summary>Note / Call / Email / ViewingBooked / StatusChange.</summary>
    public string ActivityType { get; set; } = string.Empty;

    public string? Notes { get; set; }
}

public class LeadActivityDto
{
    public int LeadActivityId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CreatedByUserEmail { get; set; }
    public DateTime CreatedDate { get; set; }
}
