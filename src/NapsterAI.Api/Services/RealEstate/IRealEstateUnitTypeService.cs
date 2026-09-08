using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateUnitTypeService
{
    Task<IReadOnlyList<UnitTypeDto>> ListAsync(int? projectId, CancellationToken cancellationToken);
    Task<UnitTypeDto> GetByIdAsync(int unitTypeId, CancellationToken cancellationToken);
    Task<UnitTypeDto> CreateAsync(CreateUnitTypeRequestDto request, CancellationToken cancellationToken);
    Task<UnitTypeDto> UpdateAsync(int unitTypeId, UpdateUnitTypeRequestDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int unitTypeId, CancellationToken cancellationToken);
}
