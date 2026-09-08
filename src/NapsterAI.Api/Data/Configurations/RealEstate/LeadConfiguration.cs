using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads", "dbo");
        builder.HasKey(l => l.LeadId);
        builder.Property(l => l.LeadId).HasColumnName("LeadID");
        builder.Property(l => l.CustomerId).HasColumnName("CustomerID");
        builder.Property(l => l.ProjectId).HasColumnName("ProjectID");
        builder.Property(l => l.InventoryId).HasColumnName("InventoryID");
        builder.Property(l => l.SalesAgentId).HasColumnName("SalesAgentID");

        builder.Property(l => l.Status).HasMaxLength(30).IsRequired();
        builder.Property(l => l.Source).HasMaxLength(30);
        builder.Property(l => l.Budget).HasColumnType("decimal(18,2)");
        builder.Property(l => l.RequirementsNotes).HasMaxLength(1000);

        builder.Property(l => l.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.SalesAgentId);
        builder.HasIndex(l => l.ProjectId);
        builder.HasIndex(l => l.CreatedDate);

        builder.HasOne(l => l.Customer)
            .WithMany()
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Project)
            .WithMany()
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Inventory)
            .WithMany()
            .HasForeignKey(l => l.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.SalesAgent)
            .WithMany()
            .HasForeignKey(l => l.SalesAgentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
