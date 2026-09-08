using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class PaymentPlanMilestoneConfiguration : IEntityTypeConfiguration<PaymentPlanMilestone>
{
    public void Configure(EntityTypeBuilder<PaymentPlanMilestone> builder)
    {
        builder.ToTable("PaymentPlanMilestones", "dbo");
        builder.HasKey(m => m.MilestoneId);
        builder.Property(m => m.MilestoneId).HasColumnName("MilestoneID");
        builder.Property(m => m.ProjectId).HasColumnName("ProjectID");

        builder.Property(m => m.MilestoneName).HasMaxLength(150).IsRequired();
        builder.Property(m => m.PercentDue).HasColumnType("decimal(5,2)");
        builder.Property(m => m.TriggerEvent).HasMaxLength(300);

        builder.HasIndex(m => m.ProjectId);

        builder.HasOne(m => m.Project)
            .WithMany()
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Mirrors Project's soft-delete filter, same as UnitTypes/Inventory/Amenities.
        builder.HasQueryFilter(m => !m.Project!.IsDeleted);
    }
}
