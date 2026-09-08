using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateProjectService
{
    Task<PagedResultDto<ProjectDto>> ListAsync(
        string? city, string? projectType, string? status, bool? isFeatured,
        decimal? minPrice, decimal? maxPrice, string? search,
        int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<ProjectDto> GetByIdAsync(int projectId, CancellationToken cancellationToken);
    Task<ProjectDto> CreateAsync(CreateProjectRequestDto request, CancellationToken cancellationToken);
    Task<ProjectDto> UpdateAsync(int projectId, UpdateProjectRequestDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int projectId, CancellationToken cancellationToken);
    Task<ProjectDto> ApproveAsync(int projectId, CancellationToken cancellationToken);
    Task<ProjectDto> RejectAsync(int projectId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UnitTypeDto>> GetUnitTypesAsync(int projectId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AmenityDto>> GetAmenitiesAsync(int projectId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MediaDto>> GetMediaAsync(int projectId, CancellationToken cancellationToken);
}
