namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>Public representation of a Project (real estate development).</summary>
public class ProjectDto
{
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Developer { get; set; }
    public string ProjectType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public int? TotalBuildings { get; set; }
    public int? TotalFloors { get; set; }
    public int? TotalUnits { get; set; }

    public DateOnly? LaunchDate { get; set; }
    public DateOnly? ConstructionStart { get; set; }
    public DateOnly? EstimatedCompletion { get; set; }
    public DateOnly? HandoverDate { get; set; }

    public decimal? StartingPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? PaymentPlan { get; set; }
    public string? PermitNumber { get; set; }
    public decimal? ServiceCharge { get; set; }

    public string? MasterPlanUrl { get; set; }
    public string? BrochureUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }

    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

/// <summary>Fields accepted when creating a Project. Server assigns ProjectId/ApprovalStatus/audit columns.</summary>
public class CreateProjectRequestDto
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Developer { get; set; }
    public string ProjectType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public int? TotalBuildings { get; set; }
    public int? TotalFloors { get; set; }
    public int? TotalUnits { get; set; }

    public DateOnly? LaunchDate { get; set; }
    public DateOnly? ConstructionStart { get; set; }
    public DateOnly? EstimatedCompletion { get; set; }
    public DateOnly? HandoverDate { get; set; }

    public decimal? StartingPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? PaymentPlan { get; set; }
    public string? PermitNumber { get; set; }
    public decimal? ServiceCharge { get; set; }

    public string? MasterPlanUrl { get; set; }
    public string? BrochureUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }

    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public bool IsFeatured { get; set; }
}

/// <summary>Fields accepted when updating a Project (full replace of the editable fields).</summary>
public class UpdateProjectRequestDto : CreateProjectRequestDto
{
    public bool IsActive { get; set; } = true;
}
