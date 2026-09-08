namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>Amenity/facility offered by a project, extending dbo.Amenities with a real FK to Project.</summary>
public class Amenity : AuditableEntity
{
    public int AmenityId { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public string AmenityName { get; set; } = string.Empty;

    /// <summary>Recreational / Security / Wellness / Convenience / Business.</summary>
    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsHighlighted { get; set; }
    public int DisplayOrder { get; set; }
}

public static class RealEstateAmenityCategories
{
    public static readonly string[] All = ["Recreational", "Security", "Wellness", "Convenience", "Business"];
}
