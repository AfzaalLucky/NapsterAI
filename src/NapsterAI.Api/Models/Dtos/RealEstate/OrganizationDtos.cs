namespace NapsterAI.Api.Models.Dtos.RealEstate;

public class OrganizationDto
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
}

public class CreateOrganizationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}

public class UpdateOrganizationRequestDto : CreateOrganizationRequestDto
{
    public bool IsActive { get; set; } = true;
}
