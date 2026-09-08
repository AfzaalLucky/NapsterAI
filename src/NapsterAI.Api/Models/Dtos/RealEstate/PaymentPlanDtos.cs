namespace NapsterAI.Api.Models.Dtos.RealEstate;

public class PaymentPlanMilestoneDto
{
    public int MilestoneId { get; set; }
    public int ProjectId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public decimal PercentDue { get; set; }
    public string? TriggerEvent { get; set; }
    public int? DueDateOffsetDays { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreatePaymentPlanMilestoneRequestDto
{
    public string MilestoneName { get; set; } = string.Empty;
    public decimal PercentDue { get; set; }
    public string? TriggerEvent { get; set; }
    public int? DueDateOffsetDays { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>Request for the EdgeMCP "calculatePaymentPlan" tool - a public, read-only computation.</summary>
public class CalculatePaymentPlanRequestDto
{
    /// <summary>Percent of ListPrice paid up-front; reduces the "On booking" milestone if present, otherwise ignored. Defaults to 0 (use the plan as defined).</summary>
    public decimal? DownPaymentPercent { get; set; }
}

public class PaymentPlanScheduleDto
{
    public int InventoryId { get; set; }
    public int ProjectId { get; set; }
    public decimal ListPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public IReadOnlyList<PaymentPlanScheduleItemDto> Milestones { get; set; } = [];
}

public class PaymentPlanScheduleItemDto
{
    public string MilestoneName { get; set; } = string.Empty;
    public decimal PercentDue { get; set; }
    public decimal AmountDue { get; set; }
    public string? TriggerEvent { get; set; }
    public int? DueDateOffsetDays { get; set; }
}
