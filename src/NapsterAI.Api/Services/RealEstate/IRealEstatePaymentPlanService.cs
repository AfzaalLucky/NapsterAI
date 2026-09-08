using NapsterAI.Api.Models.Dtos.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public interface IRealEstatePaymentPlanService
{
    Task<IReadOnlyList<PaymentPlanMilestoneDto>> ListMilestonesAsync(int projectId, CancellationToken cancellationToken);
    Task<PaymentPlanMilestoneDto> AddMilestoneAsync(int projectId, CreatePaymentPlanMilestoneRequestDto request, CancellationToken cancellationToken);
    Task<PaymentPlanScheduleDto> CalculateAsync(int inventoryId, CalculatePaymentPlanRequestDto request, CancellationToken cancellationToken);
}
