using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateAgentService
{
    Task<IReadOnlyList<RealEstateAgentDto>> ListAsync(int? organizationId, CancellationToken cancellationToken);
    Task<RealEstateAgentDto> GetByIdAsync(int salesAgentId, CancellationToken cancellationToken);
    Task<RealEstateAgentDto> CreateAsync(CreateRealEstateAgentRequestDto request, CancellationToken cancellationToken);
    Task<RealEstateAgentDto> UpdateAsync(int salesAgentId, UpdateRealEstateAgentRequestDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int salesAgentId, CancellationToken cancellationToken);
}
