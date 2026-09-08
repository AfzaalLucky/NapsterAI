using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateLeadService
{
    Task<PagedResultDto<LeadDto>> ListAsync(
        string? status, int? salesAgentId, int? projectId, int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<LeadDto> GetByIdAsync(int leadId, CancellationToken cancellationToken);
    Task<LeadDto> CreateAsync(CreateLeadRequestDto request, CancellationToken cancellationToken);
    Task<LeadDto> UpdateStatusAsync(int leadId, UpdateLeadStatusRequestDto request, int? actingUserId, CancellationToken cancellationToken);
    Task<LeadDto> AssignAsync(int leadId, AssignLeadRequestDto request, int? actingUserId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LeadActivityDto>> ListActivitiesAsync(int leadId, CancellationToken cancellationToken);
    Task<LeadActivityDto> AddActivityAsync(int leadId, CreateLeadActivityRequestDto request, int? actingUserId, CancellationToken cancellationToken);
}
