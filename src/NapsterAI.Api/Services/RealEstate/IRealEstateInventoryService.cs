using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateInventoryService
{
    Task<PagedResultDto<InventoryDto>> ListAsync(
        int? projectId, int? unitTypeId, int? minBedrooms, int? maxBedrooms,
        decimal? minPrice, decimal? maxPrice, decimal? minAreaSqFt, decimal? maxAreaSqFt,
        string? status, string? viewType, string? furnishingStatus, string? search,
        string? sortBy, bool sortDescending,
        int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<InventoryDto> GetByIdAsync(int inventoryId, CancellationToken cancellationToken);
    Task<InventoryDto> CreateAsync(CreateInventoryRequestDto request, CancellationToken cancellationToken);
    Task<InventoryDto> UpdateAsync(int inventoryId, UpdateInventoryRequestDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int inventoryId, CancellationToken cancellationToken);
}
