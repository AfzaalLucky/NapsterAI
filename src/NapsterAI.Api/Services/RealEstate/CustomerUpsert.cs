using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// Shared find-or-create-by-email logic used by RealEstateInquiryService, RealEstateLeadService,
/// and RealEstateViewingService - there is no dedicated Customers CRUD API (see
/// RealEstateImplementationPlan.md §4.1/§4.2), so every entrypoint that captures customer
/// contact info upserts through here instead of duplicating the lookup in three places.
/// </summary>
internal static class CustomerUpsert
{
    public static async Task<Customer> FindOrCreateAsync(
        RealEstateDbContext db, string fullName, string email, string? phone, string? source, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidRequestException("customerName is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidRequestException("customerEmail is required.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Email == normalizedEmail, cancellationToken);

        if (customer is null)
        {
            customer = new Customer { FullName = fullName, Email = normalizedEmail, Phone = phone, Source = source };
            db.Customers.Add(customer);
        }
        else
        {
            // Keep the most recently supplied contact details rather than overwriting blindly.
            customer.FullName = fullName;
            if (!string.IsNullOrWhiteSpace(phone))
            {
                customer.Phone = phone;
            }
        }

        return customer;
    }
}
