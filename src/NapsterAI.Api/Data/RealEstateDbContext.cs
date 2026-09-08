using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data;

/// <summary>
/// EF Core data access for the Real Estate module. This is genuinely new persistence
/// infrastructure for NapsterAI.Api (the existing Napster integration proxies live data
/// and persists nothing locally). Kept as its own DbContext/connection string
/// ("RealEstateDb") rather than merged with anything else - there is nothing to merge with.
/// </summary>
public class RealEstateDbContext(DbContextOptions<RealEstateDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<UnitType> UnitTypes => Set<UnitType>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Lookup> Lookups => Set<Lookup>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<SalesAgent> SalesAgents => Set<SalesAgent>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();
    public DbSet<Viewing> Viewings => Set<Viewing>();
    public DbSet<PaymentPlanMilestone> PaymentPlanMilestones => Set<PaymentPlanMilestone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RealEstateDbContext).Assembly);
    }
}
