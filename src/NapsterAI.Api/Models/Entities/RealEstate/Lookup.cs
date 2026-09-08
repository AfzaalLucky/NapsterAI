namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// Generic reference-data table backing admin UI dropdowns for the enum-like free-text
/// fields (ProjectType, Status, Category, FurnishingStatus, ...). Not DB-enforced via FK
/// (SQL Server can't easily FK a free-text column against a filtered subset) — actual
/// enforcement stays in the service layer via static allow-list arrays, matching the
/// idiom already used in Services\NapsterService.cs.
/// </summary>
public class Lookup
{
    public int LookupId { get; set; }

    /// <summary>ProjectType / ProjectStatus / InventoryStatus / UnitCategory / FurnishingStatus / AmenityCategory / ViewType.</summary>
    public string LookupType { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public static class RealEstateLookupTypes
{
    public const string ProjectType = "ProjectType";
    public const string ProjectStatus = "ProjectStatus";
    public const string InventoryStatus = "InventoryStatus";
    public const string UnitCategory = "UnitCategory";
    public const string FurnishingStatus = "FurnishingStatus";
    public const string AmenityCategory = "AmenityCategory";
    public const string ViewType = "ViewType";

    public static readonly string[] All =
    [
        ProjectType, ProjectStatus, InventoryStatus, UnitCategory,
        FurnishingStatus, AmenityCategory, ViewType
    ];
}
