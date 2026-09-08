namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>Public representation of a UnitType (floor-plan category within a Project).</summary>
public class UnitTypeDto
{
    public int UnitTypeId { get; set; }
    public int ProjectId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public decimal? MinAreaSqFt { get; set; }
    public decimal? MaxAreaSqFt { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? PricePerSqFt { get; set; }
    public int? TotalUnitsOfType { get; set; }
    public int? AvailableUnitsOfType { get; set; }
    public string? FloorPlanUrl { get; set; }
    public string? Description { get; set; }
}

public class CreateUnitTypeRequestDto
{
    public int ProjectId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public decimal? MinAreaSqFt { get; set; }
    public decimal? MaxAreaSqFt { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? PricePerSqFt { get; set; }
    public int? TotalUnitsOfType { get; set; }
    public int? AvailableUnitsOfType { get; set; }
    public string? FloorPlanUrl { get; set; }
    public string? Description { get; set; }
}

public class UpdateUnitTypeRequestDto : CreateUnitTypeRequestDto
{
}
