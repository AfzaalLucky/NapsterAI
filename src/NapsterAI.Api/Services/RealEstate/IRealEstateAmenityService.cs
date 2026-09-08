using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateAmenityService
{
    Task<IReadOnlyList<AmenityDto>> ListAsync(int? projectId, string? category, CancellationToken cancellationToken);
    Task<AmenityDto> GetByIdAsync(int amenityId, CancellationToken cancellationToken);
    Task<AmenityDto> CreateAsync(CreateAmenityRequestDto request, CancellationToken cancellationToken);
    Task<AmenityDto> UpdateAsync(int amenityId, UpdateAmenityRequestDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int amenityId, CancellationToken cancellationToken);
}
