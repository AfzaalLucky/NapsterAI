using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateLocationService
{
    Task<IReadOnlyList<LocationDto>> ListAsync(string? country, string? city, CancellationToken cancellationToken);
}
