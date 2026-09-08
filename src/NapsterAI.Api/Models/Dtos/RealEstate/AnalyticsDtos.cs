namespace NapsterAI.Api.Models.Dtos.RealEstate;

public class LeadsSummaryDto
{
    public int TotalLeads { get; set; }
    public IReadOnlyDictionary<string, int> ByStatus { get; set; } = new Dictionary<string, int>();
    public int ViewingsThisWeek { get; set; }

    /// <summary>Won / (Won + Lost), as a percentage. Null if no leads have reached either terminal status yet.</summary>
    public decimal? ConversionRatePercent { get; set; }

    /// <summary>Sum of Budget across leads still in an active (non-terminal) status.</summary>
    public decimal PipelineValue { get; set; }
}

public class InventorySummaryDto
{
    public int TotalProjects { get; set; }
    public int TotalUnits { get; set; }
    public IReadOnlyDictionary<string, int> ByStatus { get; set; } = new Dictionary<string, int>();

    /// <summary>Sum of ListPrice across all units, regardless of currency - see note on RealEstateAnalyticsController.</summary>
    public decimal TotalListValue { get; set; }
}
