namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>A scheduled property viewing, backing the EdgeMCP "bookViewing" tool.</summary>
public class Viewing
{
    public int ViewingId { get; set; }

    public int LeadId { get; set; }
    public Lead? Lead { get; set; }

    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public DateTime ScheduledDate { get; set; }

    /// <summary>Requested / Confirmed / Completed / Cancelled / NoShow.</summary>
    public string Status { get; set; } = RealEstateViewingStatuses.Requested;

    public int? SalesAgentId { get; set; }
    public SalesAgent? SalesAgent { get; set; }

    public string? Notes { get; set; }
}

public static class RealEstateViewingStatuses
{
    public const string Requested = "Requested";
    public const string Confirmed = "Confirmed";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "NoShow";

    public static readonly string[] All = [Requested, Confirmed, Completed, Cancelled, NoShow];
}
