using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NapsterAI.Api.Data;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Tests.Integration;

/// <summary>
/// Boots the real ASP.NET Core pipeline (Program.cs) against an isolated in-memory
/// RealEstateDbContext instead of the dev SQL Server LocalDB, and runs under the
/// "Testing" environment so Program.cs's Development-only migrate/seed block is skipped
/// (EF Core InMemory doesn't support relational migrations). This is the first real use of
/// the `partial class Program` that was already exposed for exactly this purpose.
/// </summary>
public class RealEstateApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program.cs skips its own AddDbContext(...UseSqlServer...) call under this
        // environment name specifically so this registration doesn't have to fight it.
        builder.UseEnvironment("Testing");

        // Computed once per factory (host build), not inside the options delegate below -
        // that delegate re-runs on every DbContext instantiation (i.e. every DI scope), so a
        // Guid generated there would give each scope its own empty database instead of a
        // shared one.
        var databaseName = $"IntegrationTests-{Guid.NewGuid()}";

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<RealEstateDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }

    public async Task SeedUserAsync(string email, string password, string role)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RealEstateDbContext>();
        db.Users.Add(new User { Email = email, PasswordHash = PasswordHasher.Hash(password), Role = role });
        await db.SaveChangesAsync();
    }

    public async Task<int> SeedProjectAsync(string projectCode)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RealEstateDbContext>();
        var project = new Project
        {
            ProjectCode = projectCode, ProjectName = "Integration Test Project", ProjectType = "Residential",
            Status = "Planning", Country = "UAE", City = "Dubai", Currency = "AED"
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project.ProjectId;
    }

    /// <summary>Seeds a UnitType + Inventory unit under the given project - needed by any test
    /// that posts against an inventoryId (e.g. booking a viewing), since both are real FKs.</summary>
    public async Task<int> SeedInventoryAsync(int projectId, string unitNumber)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RealEstateDbContext>();

        var unitType = new UnitType { ProjectId = projectId, TypeName = "1 Bedroom", Category = "Apartment" };
        db.UnitTypes.Add(unitType);
        await db.SaveChangesAsync();

        var inventory = new Inventory
        {
            ProjectId = projectId, UnitTypeId = unitType.UnitTypeId, UnitNumber = unitNumber, Status = "Available"
        };
        db.Inventory.Add(inventory);
        await db.SaveChangesAsync();

        return inventory.InventoryId;
    }
}
