namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// Raw incoming contact captured from the public site's "Request info" form or the AI
/// assistant's createLead EdgeMCP tool. Distinct from Lead - an Inquiry is the unqualified
/// first touch; converting one produces a Lead that an agent actually works.
/// </summary>
public class Inquiry
{
    public int InquiryId { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    /// <summary>Website / AI Assistant / Call / WalkIn - see <see cref="RealEstateLeadSources"/>.</summary>
    public string Channel { get; set; } = string.Empty;

    public string? Message { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? ConvertedToLeadId { get; set; }
    public Lead? ConvertedToLead { get; set; }
}
