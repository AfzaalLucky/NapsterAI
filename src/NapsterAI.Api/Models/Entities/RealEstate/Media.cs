namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// A gallery item attached to a Project or Inventory unit. Replaces the single-scalar
/// ImageURL/VideoURL/MasterPlanURL/BrochureURL/FloorPlanURL columns with a real
/// many-per-entity gallery; those legacy scalar columns are kept for compatibility.
/// EntityType/EntityId form a lightweight polymorphic association (no FK, since it
/// can point at more than one table) enforced in the service layer instead.
/// </summary>
public class Media
{
    public int MediaId { get; set; }

    /// <summary>"Project" or "Inventory" (see <see cref="RealEstateMediaEntityTypes"/>).</summary>
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    /// <summary>Image / Video / Document.</summary>
    public string MediaType { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public static class RealEstateMediaEntityTypes
{
    public const string Project = "Project";
    public const string Inventory = "Inventory";

    public static readonly string[] All = [Project, Inventory];
}

public static class RealEstateMediaTypes
{
    public const string Image = "Image";
    public const string Video = "Video";
    public const string Document = "Document";

    public static readonly string[] All = [Image, Video, Document];
}
