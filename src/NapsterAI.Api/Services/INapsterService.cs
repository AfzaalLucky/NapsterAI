using NapsterAI.Api.Models.Dtos;

namespace NapsterAI.Api.Services;

public interface INapsterService
{
    Task<PagedResultDto<CompanionDto>> BrowseCompanionsAsync(
        string? search, string? gender, string? ethnicity, int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<AgentDto> CreateAgentAsync(CreateAgentRequestDto request, CancellationToken cancellationToken);

    Task<ConnectionDto> CreateConnectionAsync(CreateConnectionRequestDto request, CancellationToken cancellationToken);

    Task<PagedResultDto<SessionDto>> ListSessionsAsync(
        string? companionId, string? externalClientId, string? sessionType, string? search,
        int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<PagedResultDto<KnowledgeBaseDto>> ListKnowledgeBasesAsync(
        string? provider, string? search, int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<KnowledgeBaseDto> CreateKnowledgeBaseAsync(CreateKnowledgeBaseRequestDto request, CancellationToken cancellationToken);

    Task<PagedResultDto<FaqCollectionDto>> ListFaqCollectionsAsync(
        string? search, int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<FaqCollectionDto> CreateFaqCollectionAsync(CreateFaqCollectionRequestDto request, CancellationToken cancellationToken);

    Task<PagedResultDto<FaqItemDto>> ListFaqItemsAsync(
        string faqCollectionId, int pageIndex, int pageSize, CancellationToken cancellationToken);
}
