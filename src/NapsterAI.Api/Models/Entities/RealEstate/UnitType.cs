namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// A floor-plan category within a project (Studio, 1BR, Penthouse, etc.),
/// extending dbo.UnitTypes with a real FK to Project.
/// </summary>
public class UnitType : AuditableEntity
{
    public int UnitTypeId { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    /// <summary>Studio / 1 Bedroom / 2 Bedroom / Penthouse / Townhouse.</summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>Apartment / Villa / Townhouse / Duplex / Penthouse.</summary>
    public string Category { get; set; } = string.Empty;

    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public decimal? MinAreaSqFt { get; set; }
    public decimal? MaxAreaSqFt { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? PricePerSqFt { get; set; }
    public int? TotalUnitsOfType { get; set; }
    public int? AvailableUnitsOfType { get; set; }
    public string? FloorPlanUrl { get; set; }
    public string? Description { get; set; }

    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}

public static class RealEstateUnitCategories
{
    public static readonly string[] All = ["Apartment", "Villa", "Townhouse", "Duplex", "Penthouse", "Office", "Retail", "Hotel"];
}
