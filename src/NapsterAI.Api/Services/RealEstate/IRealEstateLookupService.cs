using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateLookupService
{
    Task<IReadOnlyList<LookupDto>> ListAsync(string? lookupType, CancellationToken cancellationToken);
}
