namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// Normalized location reference data (distinct country/city/district combinations),
/// used to power search filters and autocomplete on the public site and admin dashboard.
/// </summary>
public class Location
{
    public int LocationId { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
