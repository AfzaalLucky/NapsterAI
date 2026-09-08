using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstateAnalyticsService
{
    Task<LeadsSummaryDto> GetLeadsSummaryAsync(CancellationToken cancellationToken);
    Task<InventorySummaryDto> GetInventorySummaryAsync(CancellationToken cancellationToken);
}
