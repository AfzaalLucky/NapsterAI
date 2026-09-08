namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>Public representation of an Amenity offered by a Project.</summary>
public class AmenityDto
{
    public int AmenityId { get; set; }
    public int ProjectId { get; set; }
    public string AmenityName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsHighlighted { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateAmenityRequestDto
{
    public int ProjectId { get; set; }
    public string AmenityName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsHighlighted { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateAmenityRequestDto : CreateAmenityRequestDto
{
}
