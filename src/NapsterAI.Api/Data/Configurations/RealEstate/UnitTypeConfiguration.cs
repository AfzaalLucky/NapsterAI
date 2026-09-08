using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class UnitTypeConfiguration : IEntityTypeConfiguration<UnitType>
{
    public void Configure(EntityTypeBuilder<UnitType> builder)
    {
        builder.ToTable("UnitTypes", "dbo");
        builder.HasKey(u => u.UnitTypeId);
        builder.Property(u => u.UnitTypeId).HasColumnName("UnitTypeID");
        builder.Property(u => u.ProjectId).HasColumnName("ProjectID");

        builder.Property(u => u.TypeName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Category).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Bathrooms).HasColumnType("decimal(3,1)");
        builder.Property(u => u.MinAreaSqFt).HasColumnType("decimal(10,2)");
        builder.Property(u => u.MaxAreaSqFt).HasColumnType("decimal(10,2)");
        builder.Property(u => u.BasePrice).HasColumnType("decimal(18,2)");
        builder.Property(u => u.PricePerSqFt).HasColumnType("decimal(10,2)");
        builder.Property(u => u.FloorPlanUrl).HasColumnName("FloorPlanURL").HasMaxLength(500);
        builder.Property(u => u.Description).HasMaxLength(1000);

        builder.Property(u => u.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(u => u.ProjectId);

        builder.HasOne(u => u.Project)
            .WithMany(p => p.UnitTypes)
            .HasForeignKey(u => u.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Mirrors Project's soft-delete filter so a deleted project's unit types disappear too.
        builder.HasQueryFilter(u => !u.Project!.IsDeleted);
    }
}
