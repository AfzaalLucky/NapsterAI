using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateViewingService
{
    Task<PagedResultDto<ViewingDto>> ListAsync(
        int? leadId, int? inventoryId, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<ViewingDto> GetByIdAsync(int viewingId, CancellationToken cancellationToken);
    Task<ViewingDto> CreateAsync(CreateViewingRequestDto request, CancellationToken cancellationToken);
    Task<ViewingDto> UpdateStatusAsync(int viewingId, UpdateViewingStatusRequestDto request, CancellationToken cancellationToken);
}
