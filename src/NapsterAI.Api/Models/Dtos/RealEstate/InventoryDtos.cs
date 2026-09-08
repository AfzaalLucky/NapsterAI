namespace NapsterAI.Api.Models.Dtos.RealEstate;

/// <summary>Public representation of an Inventory row (an individual sellable/leasable unit).</summary>
public class InventoryDto
{
    public int InventoryId { get; set; }
    public int ProjectId { get; set; }
    public int UnitTypeId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string? BuildingTower { get; set; }
    public int? FloorNumber { get; set; }
    public string? ViewType { get; set; }
    public decimal? AreaSqFt { get; set; }
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public int? ParkingSpaces { get; set; }
    public bool? HasBalcony { get; set; }
    public string? FurnishingStatus { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? PricePerSqFt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? ListingDate { get; set; }
    public DateOnly? SoldOrLeasedDate { get; set; }
    public string? BuyerTenantName { get; set; }
    public int? SalesAgentId { get; set; }
    public string? AgentName { get; set; }
    public string? AgentContact { get; set; }
    public string? Notes { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
}

public class CreateInventoryRequestDto
{
    public int ProjectId { get; set; }
    public int UnitTypeId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string? BuildingTower { get; set; }
    public int? FloorNumber { get; set; }
    public string? ViewType { get; set; }
    public decimal? AreaSqFt { get; set; }
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public int? ParkingSpaces { get; set; }
    public bool? HasBalcony { get; set; }
    public string? FurnishingStatus { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? PricePerSqFt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? ListingDate { get; set; }
    public int? SalesAgentId { get; set; }
    public string? AgentName { get; set; }
    public string? AgentContact { get; set; }
    public string? Notes { get; set; }
}

public class UpdateInventoryRequestDto : CreateInventoryRequestDto
{
    public DateOnly? SoldOrLeasedDate { get; set; }
    public string? BuyerTenantName { get; set; }
}
