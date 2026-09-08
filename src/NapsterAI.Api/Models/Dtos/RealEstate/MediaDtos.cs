namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>Public representation of a Media gallery item attached to a Project or Inventory unit.</summary>
public class MediaDto
{
    public int MediaId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}
