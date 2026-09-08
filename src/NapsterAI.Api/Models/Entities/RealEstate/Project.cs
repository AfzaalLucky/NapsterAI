namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// A real estate development (master record). Extends dbo.Projects from
/// database\RealEstateDB.sql with ApprovalStatus/IsDeleted and NOT NULL/unique
/// constraints on the fields that should always be present.
/// </summary>
public class Project : AuditableEntity
{
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Developer { get; set; }

    /// <summary>Residential / Commercial / Mixed-Use / Villa / Retail.</summary>
    public string ProjectType { get; set; } = string.Empty;

    /// <summary>Planning / Under Construction / Ready / Handover / Sold Out.</summary>
    public string Status { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public int? TotalBuildings { get; set; }
    public int? TotalFloors { get; set; }
    public int? TotalUnits { get; set; }

    public DateOnly? LaunchDate { get; set; }
    public DateOnly? ConstructionStart { get; set; }
    public DateOnly? EstimatedCompletion { get; set; }
    public DateOnly? HandoverDate { get; set; }

    public decimal? StartingPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? PaymentPlan { get; set; }

    /// <summary>RERA / DLD permit number.</summary>
    public string? PermitNumber { get; set; }

    /// <summary>Service charge per sq.ft/sq.m per year.</summary>
    public decimal? ServiceCharge { get; set; }

    public string? MasterPlanUrl { get; set; }
    public string? BrochureUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }

    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Draft / PendingReview / Approved / Rejected.</summary>
    public string ApprovalStatus { get; set; } = RealEstateApprovalStatuses.Draft;

    public bool IsDeleted { get; set; }

    public ICollection<UnitType> UnitTypes { get; set; } = new List<UnitType>();
    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
}

public static class RealEstateApprovalStatuses
{
    public const string Draft = "Draft";
    public const string PendingReview = "PendingReview";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static readonly string[] All = [Draft, PendingReview, Approved, Rejected];
}

public static class RealEstateProjectTypes
{
    public static readonly string[] All = ["Residential", "Commercial", "Mixed-Use", "Villa", "Retail"];
}

public static class RealEstateProjectStatuses
{
    public static readonly string[] All = ["Planning", "Under Construction", "Ready", "Handover", "Sold Out"];
}
