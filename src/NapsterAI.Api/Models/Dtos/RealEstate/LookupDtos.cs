namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>A single reference-data entry (e.g. one ProjectType or InventoryStatus option) for admin UI dropdowns.</summary>
public class LookupDto
{
    public int LookupId { get; set; }
    public string LookupType { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
