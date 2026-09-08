using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class LeadActivityConfiguration : IEntityTypeConfiguration<LeadActivity>
{
    public void Configure(EntityTypeBuilder<LeadActivity> builder)
    {
        builder.ToTable("LeadActivities", "dbo");
        builder.HasKey(a => a.LeadActivityId);
        builder.Property(a => a.LeadActivityId).HasColumnName("LeadActivityID");
        builder.Property(a => a.LeadId).HasColumnName("LeadID");
        builder.Property(a => a.CreatedByUserId).HasColumnName("CreatedByUserID");

        builder.Property(a => a.ActivityType).HasMaxLength(30).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(a => a.LeadId);

        // Cascade - an activity timeline entry is meaningless without its parent Lead.
        builder.HasOne(a => a.Lead)
            .WithMany(l => l.Activities)
            .HasForeignKey(a => a.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
