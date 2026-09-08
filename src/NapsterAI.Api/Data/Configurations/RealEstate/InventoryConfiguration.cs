using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventory", "dbo");
        builder.HasKey(i => i.InventoryId);
        builder.Property(i => i.InventoryId).HasColumnName("InventoryID");
        builder.Property(i => i.ProjectId).HasColumnName("ProjectID");
        builder.Property(i => i.UnitTypeId).HasColumnName("UnitTypeID");
        builder.Property(i => i.SalesAgentId).HasColumnName("SalesAgentID");

        builder.Property(i => i.UnitNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.BuildingTower).HasMaxLength(100);
        builder.Property(i => i.ViewType).HasMaxLength(100);
        builder.Property(i => i.AreaSqFt).HasColumnType("decimal(10,2)");
        builder.Property(i => i.Bathrooms).HasColumnType("decimal(3,1)");
        builder.Property(i => i.FurnishingStatus).HasMaxLength(50);
        builder.Property(i => i.ListPrice).HasColumnType("decimal(18,2)");
        builder.Property(i => i.PricePerSqFt).HasColumnType("decimal(10,2)");
        builder.Property(i => i.Status).HasMaxLength(50).IsRequired();
        builder.Property(i => i.BuyerTenantName).HasMaxLength(150);
        builder.Property(i => i.AgentName).HasMaxLength(150);
        builder.Property(i => i.AgentContact).HasMaxLength(50);
        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.Property(i => i.ApprovalStatus).HasMaxLength(30).IsRequired();

        builder.Property(i => i.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(i => new { i.ProjectId, i.UnitNumber }).IsUnique();
        builder.HasIndex(i => i.ProjectId);
        builder.HasIndex(i => i.UnitTypeId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.ListPrice);
        builder.HasIndex(i => i.Bedrooms);
        builder.HasIndex(i => i.SalesAgentId);

        builder.HasOne(i => i.Project)
            .WithMany(p => p.Inventory)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.UnitType)
            .WithMany(u => u.Inventory)
            .HasForeignKey(i => i.UnitTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.SalesAgent)
            .WithMany()
            .HasForeignKey(i => i.SalesAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Mirrors Project's soft-delete filter so a deleted project's inventory disappears too.
        builder.HasQueryFilter(i => !i.Project!.IsDeleted);
    }
}
