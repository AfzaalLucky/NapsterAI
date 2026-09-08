namespace NapsterAI.Api.Models.Dtos.RealEstate;

public class ViewingDto
{
    public int ViewingId { get; set; }
    public int LeadId { get; set; }
    public int InventoryId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? SalesAgentId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Public "book a viewing" submission and the EdgeMCP "bookViewing" tool's request shape.
/// Anonymous (no login) - the service transactionally upserts Customer + Lead + Viewing so
/// the AI agent doesn't have to chain a separate createLead call first.
/// </summary>
public class CreateViewingRequestDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public int InventoryId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateViewingStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
}
