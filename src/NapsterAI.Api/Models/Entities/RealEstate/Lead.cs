namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>A qualified sales opportunity an agent actively works, through to Won/Lost.</summary>
public class Lead
{
    public int LeadId { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public int? SalesAgentId { get; set; }
    public SalesAgent? SalesAgent { get; set; }

    /// <summary>New / Contacted / Qualified / ViewingScheduled / Negotiation / Won / Lost.</summary>
    public string Status { get; set; } = RealEstateLeadStatuses.New;

    /// <summary>Website / AI Assistant / Call / WalkIn - see <see cref="RealEstateLeadSources"/>.</summary>
    public string? Source { get; set; }

    public decimal? Budget { get; set; }
    public string? RequirementsNotes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastContactedDate { get; set; }

    public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
}

public static class RealEstateLeadStatuses
{
    public const string New = "New";
    public const string Contacted = "Contacted";
    public const string Qualified = "Qualified";
    public const string ViewingScheduled = "ViewingScheduled";
    public const string Negotiation = "Negotiation";
    public const string Won = "Won";
    public const string Lost = "Lost";

    public static readonly string[] All = [New, Contacted, Qualified, ViewingScheduled, Negotiation, Won, Lost];
}
