namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// An individual sellable/leasable unit, extending dbo.Inventory with real FKs
/// to Project/UnitType/SalesAgent and an ApprovalStatus for the admin approval workflow.
/// AgentName/AgentContact are kept as a legacy free-text bridge alongside SalesAgentId.
/// </summary>
public class Inventory : AuditableEntity
{
    public int InventoryId { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int UnitTypeId { get; set; }
    public UnitType? UnitType { get; set; }

    public string UnitNumber { get; set; } = string.Empty;
    public string? BuildingTower { get; set; }
    public int? FloorNumber { get; set; }

    /// <summary>Sea View / City View / Garden View / Pool View.</summary>
    public string? ViewType { get; set; }

    public decimal? AreaSqFt { get; set; }
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public int? ParkingSpaces { get; set; }
    public bool? HasBalcony { get; set; }

    /// <summary>Furnished / Semi-Furnished / Unfurnished.</summary>
    public string? FurnishingStatus { get; set; }

    public decimal? ListPrice { get; set; }
    public decimal? PricePerSqFt { get; set; }

    /// <summary>Available / Reserved / Sold / Blocked / Leased.</summary>
    public string Status { get; set; } = string.Empty;

    public DateOnly? ListingDate { get; set; }
    public DateOnly? SoldOrLeasedDate { get; set; }
    public string? BuyerTenantName { get; set; }

    public int? SalesAgentId { get; set; }
    public SalesAgent? SalesAgent { get; set; }

    // Legacy free-text agent fields, kept as a bridge alongside SalesAgentId.
    public string? AgentName { get; set; }
    public string? AgentContact { get; set; }

    public string? Notes { get; set; }

    /// <summary>Draft / PendingReview / Approved / Rejected.</summary>
    public string ApprovalStatus { get; set; } = RealEstateApprovalStatuses.Draft;
}

public static class RealEstateInventoryStatuses
{
    public const string Available = "Available";
    public const string Reserved = "Reserved";
    public const string Sold = "Sold";
    public const string Blocked = "Blocked";
    public const string Leased = "Leased";

    public static readonly string[] All = [Available, Reserved, Sold, Blocked, Leased];
}
