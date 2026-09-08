using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateOrganizationService
{
    Task<IReadOnlyList<OrganizationDto>> ListAsync(CancellationToken cancellationToken);
    Task<OrganizationDto> GetByIdAsync(int organizationId, CancellationToken cancellationToken);
    Task<OrganizationDto> CreateAsync(CreateOrganizationRequestDto request, CancellationToken cancellationToken);
    Task<OrganizationDto> UpdateAsync(int organizationId, UpdateOrganizationRequestDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int organizationId, CancellationToken cancellationToken);
}
