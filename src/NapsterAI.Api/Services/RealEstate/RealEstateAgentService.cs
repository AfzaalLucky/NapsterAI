using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public class RealEstateAgentService : IRealEstateAgentService
{
    private readonly RealEstateDbContext _db;

    public RealEstateAgentService(RealEstateDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RealEstateAgentDto>> ListAsync(int? organizationId, CancellationToken cancellationToken)
    {
        var query = _db.SalesAgents.AsNoTracking().Include(a => a.Organization).AsQueryable();
        if (organizationId is not null) query = query.Where(a => a.OrganizationId == organizationId);

        return await query
            .OrderBy(a => a.FullName)
            .Select(a => Map(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<RealEstateAgentDto> GetByIdAsync(int salesAgentId, CancellationToken cancellationToken)
    {
        return Map(await FindAsync(salesAgentId, cancellationToken));
    }

    public async Task<RealEstateAgentDto> CreateAsync(CreateRealEstateAgentRequestDto request, CancellationToken cancellationToken)
    {
        await ValidateWriteAsync(request, cancellationToken);

        var agent = new SalesAgent();
        ApplyToEntity(request, agent);

        _db.SalesAgents.Add(agent);
        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(agent).Reference(a => a.Organization).LoadAsync(cancellationToken);
        return Map(agent);
    }

    public async Task<RealEstateAgentDto> UpdateAsync(int salesAgentId, UpdateRealEstateAgentRequestDto request, CancellationToken cancellationToken)
    {
        await ValidateWriteAsync(request, cancellationToken);

        var agent = await FindAsync(salesAgentId, cancellationToken);
        ApplyToEntity(request, agent);
        agent.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);

        return Map(agent);
    }

    public async Task DeleteAsync(int salesAgentId, CancellationToken cancellationToken)
    {
        var agent = await FindAsync(salesAgentId, cancellationToken);

        var isReferenced = await _db.Inventory.AnyAsync(i => i.SalesAgentId == salesAgentId, cancellationToken)
            || await _db.Leads.AnyAsync(l => l.SalesAgentId == salesAgentId, cancellationToken)
            || await _db.Users.AnyAsync(u => u.SalesAgentId == salesAgentId, cancellationToken);

        if (isReferenced)
        {
            throw new RealEstateConflictException(
                $"Sales agent {salesAgentId} is still referenced by inventory, leads, or a login account and cannot be deleted. Deactivate it instead.");
        }

        _db.SalesAgents.Remove(agent);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<SalesAgent> FindAsync(int salesAgentId, CancellationToken cancellationToken)
    {
        var agent = await _db.SalesAgents.Include(a => a.Organization)
            .FirstOrDefaultAsync(a => a.SalesAgentId == salesAgentId, cancellationToken);

        if (agent is null)
        {
            throw new RealEstateResourceNotFoundException($"Sales agent {salesAgentId} was not found.");
        }

        return agent;
    }

    private async Task ValidateWriteAsync(CreateRealEstateAgentRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidRequestException("fullName is required.");
        }

        if (!await _db.Organizations.AnyAsync(o => o.OrganizationId == request.OrganizationId, cancellationToken))
        {
            throw new InvalidRequestException($"organizationId {request.OrganizationId} does not reference an existing organization.");
        }
    }

    private static void ApplyToEntity(CreateRealEstateAgentRequestDto request, SalesAgent agent)
    {
        agent.OrganizationId = request.OrganizationId;
        agent.FullName = request.FullName;
        agent.Phone = request.Phone;
        agent.Email = request.Email;
        agent.PhotoUrl = request.PhotoUrl;
        agent.LicenseNumber = request.LicenseNumber;
    }

    private static RealEstateAgentDto Map(SalesAgent a) => new()
    {
        SalesAgentId = a.SalesAgentId,
        OrganizationId = a.OrganizationId,
        OrganizationName = a.Organization?.Name,
        FullName = a.FullName,
        Phone = a.Phone,
        Email = a.Email,
        PhotoUrl = a.PhotoUrl,
        LicenseNumber = a.LicenseNumber,
        IsActive = a.IsActive
    };
}
