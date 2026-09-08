namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>One entry in a Lead's CRM activity timeline.</summary>
public class LeadActivity
{
    public int LeadActivityId { get; set; }

    public int LeadId { get; set; }
    public Lead? Lead { get; set; }

    /// <summary>Note / Call / Email / ViewingBooked / StatusChange.</summary>
    public string ActivityType { get; set; } = string.Empty;

    public string? Notes { get; set; }

    /// <summary>Nullable because system-generated activities (e.g. an anonymous public booking) have no acting user.</summary>
    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public static class RealEstateLeadActivityTypes
{
    public const string Note = "Note";
    public const string Call = "Call";
    public const string Email = "Email";
    public const string ViewingBooked = "ViewingBooked";
    public const string StatusChange = "StatusChange";

    public static readonly string[] All = [Note, Call, Email, ViewingBooked, StatusChange];
}
