using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// Backs the EdgeMCP "bookViewing" tool and the public site's "Book a viewing" form (both
/// anonymous - see RealEstateViewingsController). CreateAsync transactionally upserts
/// Customer + (find-or-create) Lead + Viewing in one call, so the AI agent doesn't have to
/// chain a separate createLead call first - all adds are batched onto one SaveChangesAsync,
/// which EF Core wraps in a single database transaction for a relational provider.
/// </summary>
public class RealEstateViewingService : IRealEstateViewingService
{
    private static readonly string[] TerminalLeadStatuses = [RealEstateLeadStatuses.Won, RealEstateLeadStatuses.Lost];

    private readonly RealEstateDbContext _db;
    private readonly ILogger<RealEstateViewingService> _logger;

    public RealEstateViewingService(RealEstateDbContext db, ILogger<RealEstateViewingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResultDto<ViewingDto>> ListAsync(
        int? leadId, int? inventoryId, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        var query = _db.Viewings.AsNoTracking().AsQueryable();
        if (leadId is not null) query = query.Where(v => v.LeadId == leadId);
        if (inventoryId is not null) query = query.Where(v => v.InventoryId == inventoryId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(v => v.Status == status);

        var totalCount = await _db.Viewings.CountAsync(cancellationToken);
        var filteredCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(v => v.ScheduledDate)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(v => Map(v))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ViewingDto>
        {
            Items = items,
            TotalCount = totalCount,
            FilteredCount = filteredCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<ViewingDto> GetByIdAsync(int viewingId, CancellationToken cancellationToken)
    {
        return Map(await FindAsync(viewingId, cancellationToken));
    }

    public async Task<ViewingDto> CreateAsync(CreateViewingRequestDto request, CancellationToken cancellationToken)
    {
        var inventory = await _db.Inventory.FirstOrDefaultAsync(i => i.InventoryId == request.InventoryId, cancellationToken);
        if (inventory is null)
        {
            throw new InvalidRequestException($"inventoryId {request.InventoryId} does not reference an existing inventory unit.");
        }

        if (request.ScheduledDate <= DateTime.UtcNow)
        {
            throw new InvalidRequestException("scheduledDate must be in the future.");
        }

        var customer = await CustomerUpsert.FindOrCreateAsync(
            _db, request.CustomerName, request.CustomerEmail, request.CustomerPhone, RealEstateLeadSources.Website, cancellationToken);

        // Reuse an existing non-terminal lead for this customer+project if one exists, rather
        // than fragmenting the customer's CRM history across multiple leads for the same project.
        var lead = customer.CustomerId != 0
            ? await _db.Leads.FirstOrDefaultAsync(
                l => l.CustomerId == customer.CustomerId && l.ProjectId == inventory.ProjectId && !TerminalLeadStatuses.Contains(l.Status),
                cancellationToken)
            : null;

        if (lead is null)
        {
            lead = new Lead
            {
                Customer = customer,
                ProjectId = inventory.ProjectId,
                InventoryId = inventory.InventoryId,
                Status = RealEstateLeadStatuses.ViewingScheduled,
                Source = RealEstateLeadSources.Website
            };
            _db.Leads.Add(lead);
        }
        else if (lead.Status is RealEstateLeadStatuses.New or RealEstateLeadStatuses.Contacted)
        {
            lead.Status = RealEstateLeadStatuses.ViewingScheduled;
        }

        var viewing = new Viewing
        {
            Lead = lead,
            InventoryId = request.InventoryId,
            ScheduledDate = request.ScheduledDate,
            Status = RealEstateViewingStatuses.Requested,
            Notes = request.Notes
        };
        _db.Viewings.Add(viewing);

        // Same SaveChangesAsync call as the Customer/Lead adds above - one implicit transaction.
        await _db.SaveChangesAsync(cancellationToken);

        _db.LeadActivities.Add(new LeadActivity
        {
            LeadId = lead.LeadId,
            ActivityType = RealEstateLeadActivityTypes.ViewingBooked,
            Notes = $"Viewing requested for {request.ScheduledDate:yyyy-MM-dd HH:mm} UTC (unit {inventory.UnitNumber}).",
            CreatedByUserId = null
        });
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created viewing {ViewingId} for lead {LeadId} / inventory {InventoryId}", viewing.ViewingId, lead.LeadId, request.InventoryId);

        return Map(viewing);
    }

    public async Task<ViewingDto> UpdateStatusAsync(int viewingId, UpdateViewingStatusRequestDto request, CancellationToken cancellationToken)
    {
        if (!RealEstateViewingStatuses.All.Contains(request.Status))
        {
            throw new InvalidRequestException($"status must be one of: {string.Join(", ", RealEstateViewingStatuses.All)}.");
        }

        var viewing = await FindAsync(viewingId, cancellationToken);
        viewing.Status = request.Status;

        if (request.Status == RealEstateViewingStatuses.Completed)
        {
            var lead = await _db.Leads.FirstOrDefaultAsync(l => l.LeadId == viewing.LeadId, cancellationToken);
            if (lead is not null && lead.Status == RealEstateLeadStatuses.ViewingScheduled)
            {
                lead.Status = RealEstateLeadStatuses.Negotiation;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Map(viewing);
    }

    private async Task<Viewing> FindAsync(int viewingId, CancellationToken cancellationToken)
    {
        var viewing = await _db.Viewings.FirstOrDefaultAsync(v => v.ViewingId == viewingId, cancellationToken);
        if (viewing is null)
        {
            throw new RealEstateResourceNotFoundException($"Viewing {viewingId} was not found.");
        }

        return viewing;
    }

    private static void ValidatePaging(int pageIndex, int pageSize, int maxPageSize)
    {
        if (pageIndex < 0)
        {
            throw new InvalidRequestException("pageIndex must be 0 or greater.");
        }

        if (pageSize is < 1 || pageSize > maxPageSize)
        {
            throw new InvalidRequestException($"pageSize must be between 1 and {maxPageSize}.");
        }
    }

    private static ViewingDto Map(Viewing v) => new()
    {
        ViewingId = v.ViewingId,
        LeadId = v.LeadId,
        InventoryId = v.InventoryId,
        ScheduledDate = v.ScheduledDate,
        Status = v.Status,
        SalesAgentId = v.SalesAgentId,
        Notes = v.Notes
    };
}
