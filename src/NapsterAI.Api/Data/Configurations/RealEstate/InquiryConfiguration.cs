using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.ToTable("Inquiries", "dbo");
        builder.HasKey(i => i.InquiryId);
        builder.Property(i => i.InquiryId).HasColumnName("InquiryID");
        builder.Property(i => i.CustomerId).HasColumnName("CustomerID");
        builder.Property(i => i.ProjectId).HasColumnName("ProjectID");
        builder.Property(i => i.InventoryId).HasColumnName("InventoryID");
        builder.Property(i => i.ConvertedToLeadId).HasColumnName("ConvertedToLeadID");

        builder.Property(i => i.Channel).HasMaxLength(30).IsRequired();
        builder.Property(i => i.Message).HasMaxLength(2000);

        builder.Property(i => i.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(i => i.CustomerId);
        builder.HasIndex(i => i.ProjectId);
        builder.HasIndex(i => i.CreatedDate);

        builder.HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Project)
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Inventory)
            .WithMany()
            .HasForeignKey(i => i.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // No FK constraint to Lead here - see LeadConfiguration's Inquiry-side note; avoids a
        // multiple-cascade-paths error since Lead already cascades from Customer indirectly.
        builder.HasOne(i => i.ConvertedToLead)
            .WithMany()
            .HasForeignKey(i => i.ConvertedToLeadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
