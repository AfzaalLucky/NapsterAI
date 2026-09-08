using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

public class RealEstateOrganizationService : IRealEstateOrganizationService
{
    private readonly RealEstateDbContext _db;

    public RealEstateOrganizationService(RealEstateDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OrganizationDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await _db.Organizations.AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => Map(o))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganizationDto> GetByIdAsync(int organizationId, CancellationToken cancellationToken)
    {
        return Map(await FindAsync(organizationId, cancellationToken));
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationRequestDto request, CancellationToken cancellationToken)
    {
        ValidateWrite(request);

        var organization = new Organization();
        ApplyToEntity(request, organization);

        _db.Organizations.Add(organization);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(organization);
    }

    public async Task<OrganizationDto> UpdateAsync(int organizationId, UpdateOrganizationRequestDto request, CancellationToken cancellationToken)
    {
        ValidateWrite(request);

        var organization = await FindAsync(organizationId, cancellationToken);
        ApplyToEntity(request, organization);
        organization.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);

        return Map(organization);
    }

    public async Task DeleteAsync(int organizationId, CancellationToken cancellationToken)
    {
        var organization = await FindAsync(organizationId, cancellationToken);

        if (await _db.SalesAgents.AnyAsync(a => a.OrganizationId == organizationId, cancellationToken))
        {
            throw new RealEstateConflictException($"Organization {organizationId} still has sales agents and cannot be deleted.");
        }

        _db.Organizations.Remove(organization);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Organization> FindAsync(int organizationId, CancellationToken cancellationToken)
    {
        var organization = await _db.Organizations.FirstOrDefaultAsync(o => o.OrganizationId == organizationId, cancellationToken);
        if (organization is null)
        {
            throw new RealEstateResourceNotFoundException($"Organization {organizationId} was not found.");
        }

        return organization;
    }

    private static void ValidateWrite(CreateOrganizationRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidRequestException("name is required.");
        }
    }

    private static void ApplyToEntity(CreateOrganizationRequestDto request, Organization organization)
    {
        organization.Name = request.Name;
        organization.LicenseNumber = request.LicenseNumber;
        organization.LogoUrl = request.LogoUrl;
        organization.Phone = request.Phone;
        organization.Email = request.Email;
        organization.Website = request.Website;
    }

    private static OrganizationDto Map(Organization o) => new()
    {
        OrganizationId = o.OrganizationId,
        Name = o.Name,
        LicenseNumber = o.LicenseNumber,
        LogoUrl = o.LogoUrl,
        Phone = o.Phone,
        Email = o.Email,
        Website = o.Website,
        IsActive = o.IsActive
    };
}
