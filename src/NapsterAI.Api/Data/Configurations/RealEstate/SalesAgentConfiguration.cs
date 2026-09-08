using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class SalesAgentConfiguration : IEntityTypeConfiguration<SalesAgent>
{
    public void Configure(EntityTypeBuilder<SalesAgent> builder)
    {
        builder.ToTable("SalesAgents", "dbo");
        builder.HasKey(a => a.SalesAgentId);
        builder.Property(a => a.SalesAgentId).HasColumnName("SalesAgentID");
        builder.Property(a => a.OrganizationId).HasColumnName("OrganizationID");

        builder.Property(a => a.FullName).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(50);
        builder.Property(a => a.Email).HasMaxLength(150);
        builder.Property(a => a.PhotoUrl).HasMaxLength(500);
        builder.Property(a => a.LicenseNumber).HasMaxLength(100);
        builder.Property(a => a.IsActive).HasDefaultValue(true);

        builder.HasIndex(a => a.OrganizationId);

        builder.HasOne(a => a.Organization)
            .WithMany(o => o.SalesAgents)
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
