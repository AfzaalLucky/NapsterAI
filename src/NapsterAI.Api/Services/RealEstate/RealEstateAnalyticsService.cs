using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>Admin dashboard KPI aggregates. Both queries are straightforward GROUP BYs - no caching, since these are meant to reflect current state.</summary>
public class RealEstateAnalyticsService : IRealEstateAnalyticsService
{
    private readonly RealEstateDbContext _db;

    public RealEstateAnalyticsService(RealEstateDbContext db)
    {
        _db = db;
    }

    public async Task<LeadsSummaryDto> GetLeadsSummaryAsync(CancellationToken cancellationToken)
    {
        var byStatus = await _db.Leads.AsNoTracking()
            .GroupBy(l => l.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var totalLeads = byStatus.Values.Sum();
        var won = byStatus.GetValueOrDefault(RealEstateLeadStatuses.Won);
        var lost = byStatus.GetValueOrDefault(RealEstateLeadStatuses.Lost);
        var terminalCount = won + lost;

        var pipelineValue = await _db.Leads.AsNoTracking()
            .Where(l => l.Status != RealEstateLeadStatuses.Won && l.Status != RealEstateLeadStatuses.Lost)
            .SumAsync(l => l.Budget ?? 0m, cancellationToken);

        var weekFromNow = DateTime.UtcNow.AddDays(7);
        var viewingsThisWeek = await _db.Viewings.AsNoTracking()
            .CountAsync(v => v.ScheduledDate >= DateTime.UtcNow && v.ScheduledDate <= weekFromNow
                && v.Status != RealEstateViewingStatuses.Cancelled, cancellationToken);

        return new LeadsSummaryDto
        {
            TotalLeads = totalLeads,
            ByStatus = byStatus,
            ViewingsThisWeek = viewingsThisWeek,
            ConversionRatePercent = terminalCount > 0 ? Math.Round((decimal)won / terminalCount * 100m, 1) : null,
            PipelineValue = pipelineValue
        };
    }

    public async Task<InventorySummaryDto> GetInventorySummaryAsync(CancellationToken cancellationToken)
    {
        var totalProjects = await _db.Projects.AsNoTracking().CountAsync(cancellationToken);

        var byStatus = await _db.Inventory.AsNoTracking()
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var totalListValue = await _db.Inventory.AsNoTracking().SumAsync(i => i.ListPrice ?? 0m, cancellationToken);

        return new InventorySummaryDto
        {
            TotalProjects = totalProjects,
            TotalUnits = byStatus.Values.Sum(),
            ByStatus = byStatus,
            TotalListValue = totalListValue
        };
    }
}
