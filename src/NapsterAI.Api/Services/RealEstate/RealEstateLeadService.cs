using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public class RealEstateLeadService : IRealEstateLeadService
{
    private readonly RealEstateDbContext _db;
    private readonly ILogger<RealEstateLeadService> _logger;

    public RealEstateLeadService(RealEstateDbContext db, ILogger<RealEstateLeadService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResultDto<LeadDto>> ListAsync(
        string? status, int? salesAgentId, int? projectId, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        var query = _db.Leads.AsNoTracking()
            .Include(l => l.Customer)
            .Include(l => l.SalesAgent)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(l => l.Status == status);
        if (salesAgentId is not null) query = query.Where(l => l.SalesAgentId == salesAgentId);
        if (projectId is not null) query = query.Where(l => l.ProjectId == projectId);

        var totalCount = await _db.Leads.CountAsync(cancellationToken);
        var filteredCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.CreatedDate)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(l => Map(l))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<LeadDto>
        {
            Items = items,
            TotalCount = totalCount,
            FilteredCount = filteredCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<LeadDto> GetByIdAsync(int leadId, CancellationToken cancellationToken)
    {
        return Map(await FindAsync(leadId, cancellationToken));
    }

    public async Task<LeadDto> CreateAsync(CreateLeadRequestDto request, CancellationToken cancellationToken)
    {
        if (request.ProjectId is not null && !await _db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId, cancellationToken))
        {
            throw new InvalidRequestException($"projectId {request.ProjectId} does not reference an existing project.");
        }

        if (request.InventoryId is not null && !await _db.Inventory.AnyAsync(i => i.InventoryId == request.InventoryId, cancellationToken))
        {
            throw new InvalidRequestException($"inventoryId {request.InventoryId} does not reference an existing inventory unit.");
        }

        var customer = await CustomerUpsert.FindOrCreateAsync(
            _db, request.CustomerName, request.CustomerEmail, request.CustomerPhone, request.Source, cancellationToken);

        var lead = new Lead
        {
            Customer = customer,
            ProjectId = request.ProjectId,
            InventoryId = request.InventoryId,
            Status = RealEstateLeadStatuses.New,
            Source = request.Source,
            Budget = request.Budget,
            RequirementsNotes = request.RequirementsNotes
        };

        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created lead {LeadId} for customer {Email}", lead.LeadId, customer.Email);

        await _db.Entry(lead).Reference(l => l.SalesAgent).LoadAsync(cancellationToken);
        return Map(lead);
    }

    public async Task<LeadDto> UpdateStatusAsync(int leadId, UpdateLeadStatusRequestDto request, int? actingUserId, CancellationToken cancellationToken)
    {
        if (!RealEstateLeadStatuses.All.Contains(request.Status))
        {
            throw new InvalidRequestException($"status must be one of: {string.Join(", ", RealEstateLeadStatuses.All)}.");
        }

        var lead = await FindAsync(leadId, cancellationToken);
        var previousStatus = lead.Status;
        lead.Status = request.Status;
        lead.LastContactedDate = DateTime.UtcNow;

        _db.LeadActivities.Add(new LeadActivity
        {
            LeadId = leadId,
            ActivityType = RealEstateLeadActivityTypes.StatusChange,
            Notes = $"Status changed from {previousStatus} to {request.Status}.",
            CreatedByUserId = actingUserId
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Map(lead);
    }

    public async Task<LeadDto> AssignAsync(int leadId, AssignLeadRequestDto request, int? actingUserId, CancellationToken cancellationToken)
    {
        var lead = await FindAsync(leadId, cancellationToken);

        var agent = await _db.SalesAgents.FirstOrDefaultAsync(a => a.SalesAgentId == request.SalesAgentId, cancellationToken);
        if (agent is null)
        {
            throw new InvalidRequestException($"salesAgentId {request.SalesAgentId} does not reference an existing sales agent.");
        }

        lead.SalesAgentId = request.SalesAgentId;

        _db.LeadActivities.Add(new LeadActivity
        {
            LeadId = leadId,
            ActivityType = RealEstateLeadActivityTypes.Note,
            Notes = $"Assigned to {agent.FullName}.",
            CreatedByUserId = actingUserId
        });

        await _db.SaveChangesAsync(cancellationToken);

        lead.SalesAgent = agent;
        return Map(lead);
    }

    public async Task<IReadOnlyList<LeadActivityDto>> ListActivitiesAsync(int leadId, CancellationToken cancellationToken)
    {
        await FindAsync(leadId, cancellationToken);

        return await _db.LeadActivities.AsNoTracking()
            .Include(a => a.CreatedByUser)
            .Where(a => a.LeadId == leadId)
            .OrderByDescending(a => a.CreatedDate)
            .Select(a => MapActivity(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<LeadActivityDto> AddActivityAsync(int leadId, CreateLeadActivityRequestDto request, int? actingUserId, CancellationToken cancellationToken)
    {
        if (!RealEstateLeadActivityTypes.All.Contains(request.ActivityType))
        {
            throw new InvalidRequestException($"activityType must be one of: {string.Join(", ", RealEstateLeadActivityTypes.All)}.");
        }

        await FindAsync(leadId, cancellationToken);

        var activity = new LeadActivity
        {
            LeadId = leadId,
            ActivityType = request.ActivityType,
            Notes = request.Notes,
            CreatedByUserId = actingUserId
        };

        _db.LeadActivities.Add(activity);

        var lead = await _db.Leads.FirstAsync(l => l.LeadId == leadId, cancellationToken);
        lead.LastContactedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        if (actingUserId is not null)
        {
            activity.CreatedByUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == actingUserId, cancellationToken);
        }

        return MapActivity(activity);
    }

    private async Task<Lead> FindAsync(int leadId, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads
            .Include(l => l.Customer)
            .Include(l => l.SalesAgent)
            .FirstOrDefaultAsync(l => l.LeadId == leadId, cancellationToken);

        if (lead is null)
        {
            throw new RealEstateResourceNotFoundException($"Lead {leadId} was not found.");
        }

        return lead;
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

    internal static LeadDto Map(Lead l) => new()
    {
        LeadId = l.LeadId,
        Customer = MapCustomer(l.Customer),
        ProjectId = l.ProjectId,
        InventoryId = l.InventoryId,
        SalesAgentId = l.SalesAgentId,
        SalesAgentName = l.SalesAgent?.FullName,
        Status = l.Status,
        Source = l.Source,
        Budget = l.Budget,
        RequirementsNotes = l.RequirementsNotes,
        CreatedDate = l.CreatedDate,
        LastContactedDate = l.LastContactedDate
    };

    internal static CustomerDto MapCustomer(Customer? c) => c is null ? new CustomerDto() : new CustomerDto
    {
        CustomerId = c.CustomerId,
        FullName = c.FullName,
        Email = c.Email,
        Phone = c.Phone,
        Nationality = c.Nationality,
        PreferredLanguage = c.PreferredLanguage,
        Source = c.Source
    };

    private static LeadActivityDto MapActivity(LeadActivity a) => new()
    {
        LeadActivityId = a.LeadActivityId,
        ActivityType = a.ActivityType,
        Notes = a.Notes,
        CreatedByUserEmail = a.CreatedByUser?.Email,
        CreatedDate = a.CreatedDate
    };
}
