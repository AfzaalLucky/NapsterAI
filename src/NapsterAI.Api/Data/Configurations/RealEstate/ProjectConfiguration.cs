using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", "dbo");
        builder.HasKey(p => p.ProjectId);
        builder.Property(p => p.ProjectId).HasColumnName("ProjectID");

        builder.Property(p => p.ProjectCode).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ProjectName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Developer).HasMaxLength(200);
        builder.Property(p => p.ProjectType).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Status).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasColumnType("nvarchar(max)");

        builder.Property(p => p.Country).HasMaxLength(100).IsRequired();
        builder.Property(p => p.City).HasMaxLength(100).IsRequired();
        builder.Property(p => p.District).HasMaxLength(150);
        builder.Property(p => p.Address).HasMaxLength(300);
        builder.Property(p => p.Latitude).HasColumnType("decimal(9,6)");
        builder.Property(p => p.Longitude).HasColumnType("decimal(9,6)");

        builder.Property(p => p.StartingPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.MaxPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Currency).HasMaxLength(10).IsRequired();
        builder.Property(p => p.PaymentPlan).HasMaxLength(500);
        builder.Property(p => p.PermitNumber).HasMaxLength(100);
        builder.Property(p => p.ServiceCharge).HasColumnType("decimal(10,2)");

        builder.Property(p => p.MasterPlanUrl).HasColumnName("MasterPlanURL").HasMaxLength(500);
        builder.Property(p => p.BrochureUrl).HasColumnName("BrochureURL").HasMaxLength(500);
        builder.Property(p => p.ImageUrl).HasColumnName("ImageURL").HasMaxLength(500);
        builder.Property(p => p.VideoUrl).HasColumnName("VideoURL").HasMaxLength(500);

        builder.Property(p => p.ContactPerson).HasMaxLength(150);
        builder.Property(p => p.ContactPhone).HasMaxLength(50);
        builder.Property(p => p.ContactEmail).HasMaxLength(150);

        builder.Property(p => p.IsFeatured).HasDefaultValue(false);
        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.ApprovalStatus).HasMaxLength(30).IsRequired();
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);

        builder.Property(p => p.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => p.ProjectCode).IsUnique();
        builder.HasIndex(p => p.City);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.ProjectType);
        builder.HasIndex(p => p.Status);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
