namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>A distinct country/city/district combination, used to power search filters and autocomplete.</summary>
public class LocationDto
{
    public int LocationId { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
