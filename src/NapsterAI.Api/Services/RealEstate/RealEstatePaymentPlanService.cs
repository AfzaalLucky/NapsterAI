using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// Structured payment-plan milestones per project, and the calculation the EdgeMCP
/// "calculatePaymentPlan" tool calls. Replaces the free-text Projects.PaymentPlan column as
/// the actual source of truth - see PaymentPlanMilestone.cs for the rationale.
/// </summary>
public class RealEstatePaymentPlanService : IRealEstatePaymentPlanService
{
    private readonly RealEstateDbContext _db;

    public RealEstatePaymentPlanService(RealEstateDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PaymentPlanMilestoneDto>> ListMilestonesAsync(int projectId, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.ProjectId == projectId, cancellationToken))
        {
            throw new RealEstateResourceNotFoundException($"Project {projectId} was not found.");
        }

        return await _db.PaymentPlanMilestones.AsNoTracking()
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.DisplayOrder)
            .Select(m => Map(m))
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentPlanMilestoneDto> AddMilestoneAsync(int projectId, CreatePaymentPlanMilestoneRequestDto request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.ProjectId == projectId, cancellationToken))
        {
            throw new RealEstateResourceNotFoundException($"Project {projectId} was not found.");
        }

        if (string.IsNullOrWhiteSpace(request.MilestoneName))
        {
            throw new InvalidRequestException("milestoneName is required.");
        }

        if (request.PercentDue is <= 0 or > 100)
        {
            throw new InvalidRequestException("percentDue must be between 0 (exclusive) and 100.");
        }

        var milestone = new PaymentPlanMilestone
        {
            ProjectId = projectId,
            MilestoneName = request.MilestoneName,
            PercentDue = request.PercentDue,
            TriggerEvent = request.TriggerEvent,
            DueDateOffsetDays = request.DueDateOffsetDays,
            DisplayOrder = request.DisplayOrder
        };

        _db.PaymentPlanMilestones.Add(milestone);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(milestone);
    }

    public async Task<PaymentPlanScheduleDto> CalculateAsync(int inventoryId, CalculatePaymentPlanRequestDto request, CancellationToken cancellationToken)
    {
        var inventory = await _db.Inventory.Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.InventoryId == inventoryId, cancellationToken);

        if (inventory is null)
        {
            throw new RealEstateResourceNotFoundException($"Inventory unit {inventoryId} was not found.");
        }

        if (inventory.ListPrice is null)
        {
            throw new InvalidRequestException($"Inventory unit {inventoryId} has no list price set; cannot calculate a payment plan.");
        }

        var milestones = await _db.PaymentPlanMilestones.AsNoTracking()
            .Where(m => m.ProjectId == inventory.ProjectId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        if (milestones.Count == 0)
        {
            throw new RealEstateResourceNotFoundException($"No payment plan is configured for project {inventory.ProjectId}.");
        }

        var listPrice = inventory.ListPrice.Value;
        var downPaymentPercent = request.DownPaymentPercent ?? 0m;

        if (downPaymentPercent is < 0 or > 100)
        {
            throw new InvalidRequestException("downPaymentPercent must be between 0 and 100.");
        }

        var items = new List<PaymentPlanScheduleItemDto>();

        if (downPaymentPercent > 0)
        {
            items.Add(new PaymentPlanScheduleItemDto
            {
                MilestoneName = "Down Payment",
                PercentDue = downPaymentPercent,
                AmountDue = Math.Round(listPrice * downPaymentPercent / 100m, 2),
                TriggerEvent = "On booking"
            });

            // The remaining milestones are scaled proportionally so the schedule still totals
            // 100% of ListPrice after the requested down payment is carved out up front.
            var totalDefinedPercent = milestones.Sum(m => m.PercentDue);
            var remainingPercent = 100m - downPaymentPercent;

            foreach (var milestone in milestones)
            {
                var scaledPercent = totalDefinedPercent > 0 ? milestone.PercentDue / totalDefinedPercent * remainingPercent : 0m;
                items.Add(BuildItem(milestone, listPrice, Math.Round(scaledPercent, 2)));
            }
        }
        else
        {
            items.AddRange(milestones.Select(m => BuildItem(m, listPrice, m.PercentDue)));
        }

        return new PaymentPlanScheduleDto
        {
            InventoryId = inventoryId,
            ProjectId = inventory.ProjectId,
            ListPrice = listPrice,
            Currency = inventory.Project?.Currency ?? string.Empty,
            Milestones = items
        };
    }

    private static PaymentPlanScheduleItemDto BuildItem(PaymentPlanMilestone milestone, decimal listPrice, decimal percentDue) => new()
    {
        MilestoneName = milestone.MilestoneName,
        PercentDue = percentDue,
        AmountDue = Math.Round(listPrice * percentDue / 100m, 2),
        TriggerEvent = milestone.TriggerEvent,
        DueDateOffsetDays = milestone.DueDateOffsetDays
    };

    private static PaymentPlanMilestoneDto Map(PaymentPlanMilestone m) => new()
    {
        MilestoneId = m.MilestoneId,
        ProjectId = m.ProjectId,
        MilestoneName = m.MilestoneName,
        PercentDue = m.PercentDue,
        TriggerEvent = m.TriggerEvent,
        DueDateOffsetDays = m.DueDateOffsetDays,
        DisplayOrder = m.DisplayOrder
    };
}
