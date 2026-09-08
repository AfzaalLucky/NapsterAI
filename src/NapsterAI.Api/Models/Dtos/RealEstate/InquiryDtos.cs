namespace NapsterAI.Api.Models.Dtos.RealEstate;

public class InquiryDto
{
    public int InquiryId { get; set; }
    public CustomerDto Customer { get; set; } = new();
    public int? ProjectId { get; set; }
    public int? InventoryId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ConvertedToLeadId { get; set; }
}

/// <summary>
/// Public contact-us submission and the EdgeMCP "createLead" tool's request shape.
/// Anonymous (no login) - see RealEstateInquiriesController.
/// </summary>
public class CreateInquiryRequestDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public int? ProjectId { get; set; }
    public int? InventoryId { get; set; }

    /// <summary>Website / AI Assistant / Call / WalkIn.</summary>
    public string Channel { get; set; } = string.Empty;

    public string? Message { get; set; }
}
