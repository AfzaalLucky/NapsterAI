using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task<AuthResultDto> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken);
}
