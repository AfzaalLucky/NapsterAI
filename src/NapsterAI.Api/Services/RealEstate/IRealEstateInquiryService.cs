using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateInquiryService
{
    Task<PagedResultDto<InquiryDto>> ListAsync(int? projectId, int pageIndex, int pageSize, CancellationToken cancellationToken);
    Task<InquiryDto> GetByIdAsync(int inquiryId, CancellationToken cancellationToken);
    Task<InquiryDto> CreateAsync(CreateInquiryRequestDto request, CancellationToken cancellationToken);
    Task<LeadDto> ConvertToLeadAsync(int inquiryId, CancellationToken cancellationToken);
}
