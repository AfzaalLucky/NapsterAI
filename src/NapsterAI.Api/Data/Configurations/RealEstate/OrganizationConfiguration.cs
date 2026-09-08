using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations", "dbo");
        builder.HasKey(o => o.OrganizationId);
        builder.Property(o => o.OrganizationId).HasColumnName("OrganizationID");

        builder.Property(o => o.Name).HasMaxLength(200).IsRequired();
        builder.Property(o => o.LicenseNumber).HasMaxLength(100);
        builder.Property(o => o.LogoUrl).HasMaxLength(500);
        builder.Property(o => o.Phone).HasMaxLength(50);
        builder.Property(o => o.Email).HasMaxLength(150);
        builder.Property(o => o.Website).HasMaxLength(300);
        builder.Property(o => o.IsActive).HasDefaultValue(true);
    }
}
