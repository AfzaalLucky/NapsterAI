using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// Captures raw first-touch contact from the public site's "Request info" form and the
/// EdgeMCP "createLead" tool (both anonymous - see RealEstateInquiriesController). Converting
/// one produces the Lead an agent actually works.
/// </summary>
public class RealEstateInquiryService : IRealEstateInquiryService
{
    private readonly RealEstateDbContext _db;
    private readonly ILogger<RealEstateInquiryService> _logger;

    public RealEstateInquiryService(RealEstateDbContext db, ILogger<RealEstateInquiryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResultDto<InquiryDto>> ListAsync(int? projectId, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        ValidatePaging(pageIndex, pageSize, maxPageSize: 100);

        var query = _db.Inquiries.AsNoTracking().Include(i => i.Customer).AsQueryable();
        if (projectId is not null) query = query.Where(i => i.ProjectId == projectId);

        var totalCount = await _db.Inquiries.CountAsync(cancellationToken);
        var filteredCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.CreatedDate)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(i => Map(i))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<InquiryDto>
        {
            Items = items,
            TotalCount = totalCount,
            FilteredCount = filteredCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<InquiryDto> GetByIdAsync(int inquiryId, CancellationToken cancellationToken)
    {
        return Map(await FindAsync(inquiryId, cancellationToken));
    }

    public async Task<InquiryDto> CreateAsync(CreateInquiryRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Channel))
        {
            throw new InvalidRequestException("channel is required.");
        }

        if (!RealEstateLeadSources.All.Contains(request.Channel))
        {
            throw new InvalidRequestException($"channel must be one of: {string.Join(", ", RealEstateLeadSources.All)}.");
        }

        if (request.ProjectId is not null && !await _db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId, cancellationToken))
        {
            throw new InvalidRequestException($"projectId {request.ProjectId} does not reference an existing project.");
        }

        if (request.InventoryId is not null && !await _db.Inventory.AnyAsync(i => i.InventoryId == request.InventoryId, cancellationToken))
        {
            throw new InvalidRequestException($"inventoryId {request.InventoryId} does not reference an existing inventory unit.");
        }

        var customer = await CustomerUpsert.FindOrCreateAsync(
            _db, request.CustomerName, request.CustomerEmail, request.CustomerPhone, request.Channel, cancellationToken);

        var inquiry = new Inquiry
        {
            Customer = customer,
            ProjectId = request.ProjectId,
            InventoryId = request.InventoryId,
            Channel = request.Channel,
            Message = request.Message
        };

        _db.Inquiries.Add(inquiry);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created inquiry {InquiryId} from {Channel} for {Email}", inquiry.InquiryId, inquiry.Channel, customer.Email);

        return Map(inquiry);
    }

    public async Task<LeadDto> ConvertToLeadAsync(int inquiryId, CancellationToken cancellationToken)
    {
        var inquiry = await FindAsync(inquiryId, cancellationToken);

        if (inquiry.ConvertedToLeadId is not null)
        {
            throw new RealEstateConflictException($"Inquiry {inquiryId} was already converted to lead {inquiry.ConvertedToLeadId}.");
        }

        var lead = new Lead
        {
            CustomerId = inquiry.CustomerId,
            ProjectId = inquiry.ProjectId,
            InventoryId = inquiry.InventoryId,
            Status = RealEstateLeadStatuses.New,
            Source = inquiry.Channel,
            RequirementsNotes = inquiry.Message
        };

        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(cancellationToken);

        inquiry.ConvertedToLeadId = lead.LeadId;

        _db.LeadActivities.Add(new LeadActivity
        {
            LeadId = lead.LeadId,
            ActivityType = RealEstateLeadActivityTypes.Note,
            Notes = $"Converted from inquiry #{inquiry.InquiryId} ({inquiry.Channel}).",
            CreatedByUserId = null
        });

        await _db.SaveChangesAsync(cancellationToken);

        lead.Customer = inquiry.Customer;
        _logger.LogInformation("Converted inquiry {InquiryId} to lead {LeadId}", inquiryId, lead.LeadId);

        return RealEstateLeadService.Map(lead);
    }

    private async Task<Inquiry> FindAsync(int inquiryId, CancellationToken cancellationToken)
    {
        var inquiry = await _db.Inquiries.Include(i => i.Customer).FirstOrDefaultAsync(i => i.InquiryId == inquiryId, cancellationToken);
        if (inquiry is null)
        {
            throw new RealEstateResourceNotFoundException($"Inquiry {inquiryId} was not found.");
        }

        return inquiry;
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

    private static InquiryDto Map(Inquiry i) => new()
    {
        InquiryId = i.InquiryId,
        Customer = RealEstateLeadService.MapCustomer(i.Customer),
        ProjectId = i.ProjectId,
        InventoryId = i.InventoryId,
        Channel = i.Channel,
        Message = i.Message,
        CreatedDate = i.CreatedDate,
        ConvertedToLeadId = i.ConvertedToLeadId
    };
}
