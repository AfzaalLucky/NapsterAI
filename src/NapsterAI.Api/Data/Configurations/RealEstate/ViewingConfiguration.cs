using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class ViewingConfiguration : IEntityTypeConfiguration<Viewing>
{
    public void Configure(EntityTypeBuilder<Viewing> builder)
    {
        builder.ToTable("Viewings", "dbo");
        builder.HasKey(v => v.ViewingId);
        builder.Property(v => v.ViewingId).HasColumnName("ViewingID");
        builder.Property(v => v.LeadId).HasColumnName("LeadID");
        builder.Property(v => v.InventoryId).HasColumnName("InventoryID");
        builder.Property(v => v.SalesAgentId).HasColumnName("SalesAgentID");

        builder.Property(v => v.Status).HasMaxLength(30).IsRequired();
        builder.Property(v => v.Notes).HasMaxLength(1000);

        builder.HasIndex(v => v.LeadId);
        builder.HasIndex(v => v.InventoryId);
        builder.HasIndex(v => v.ScheduledDate);
        builder.HasIndex(v => v.Status);

        builder.HasOne(v => v.Lead)
            .WithMany()
            .HasForeignKey(v => v.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Inventory)
            .WithMany()
            .HasForeignKey(v => v.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.SalesAgent)
            .WithMany()
            .HasForeignKey(v => v.SalesAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Mirrors Inventory's (itself Project-derived) soft-delete filter.
        builder.HasQueryFilter(v => !v.Inventory!.Project!.IsDeleted);
    }
}
