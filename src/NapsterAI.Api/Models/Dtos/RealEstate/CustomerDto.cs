namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>
/// Read-only, embedded in InquiryDto/LeadDto responses. There is no dedicated Customers
/// CRUD API - customer records are upserted-by-email as a side effect of creating an
/// Inquiry, Lead, or Viewing (see Services\RealEstate\CustomerUpsert.cs).
/// </summary>
public class CustomerDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Nationality { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Source { get; set; }
}
