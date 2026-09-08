namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// One structured installment in a project's payment plan - replaces the free-text
/// Projects.PaymentPlan column as the source of truth so the EdgeMCP "calculatePaymentPlan"
/// tool can actually compute a schedule instead of NLP-parsing prose.
/// </summary>
public class PaymentPlanMilestone
{
    public int MilestoneId { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public string MilestoneName { get; set; } = string.Empty;

    /// <summary>Percentage of the unit's ListPrice due at this milestone (0-100).</summary>
    public decimal PercentDue { get; set; }

    /// <summary>Free-text description of what triggers this milestone, e.g. "On booking", "20% construction complete".</summary>
    public string? TriggerEvent { get; set; }

    /// <summary>Days from booking date this milestone is estimated to fall due, for schedule display. Null if event-triggered rather than date-triggered.</summary>
    public int? DueDateOffsetDays { get; set; }

    public int DisplayOrder { get; set; }
}
