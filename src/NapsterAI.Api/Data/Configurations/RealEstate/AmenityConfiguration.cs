using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        builder.ToTable("Amenities", "dbo");
        builder.HasKey(a => a.AmenityId);
        builder.Property(a => a.AmenityId).HasColumnName("AmenityID");
        builder.Property(a => a.ProjectId).HasColumnName("ProjectID");

        builder.Property(a => a.AmenityName).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Category).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.IconUrl).HasColumnName("IconURL").HasMaxLength(500);
        builder.Property(a => a.ImageUrl).HasColumnName("ImageURL").HasMaxLength(500);

        builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(a => a.ProjectId);
        builder.HasIndex(a => a.Category);

        builder.HasOne(a => a.Project)
            .WithMany(p => p.Amenities)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Mirrors Project's soft-delete filter so a deleted project's amenities disappear too.
        builder.HasQueryFilter(a => !a.Project!.IsDeleted);
    }
}
