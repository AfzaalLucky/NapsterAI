using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.ToTable("Media", "dbo");
        builder.HasKey(m => m.MediaId);
        builder.Property(m => m.MediaId).HasColumnName("MediaID");

        builder.Property(m => m.EntityType).HasMaxLength(30).IsRequired();
        builder.Property(m => m.MediaType).HasMaxLength(30).IsRequired();
        builder.Property(m => m.Url).HasMaxLength(500).IsRequired();

        builder.Property(m => m.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(m => new { m.EntityType, m.EntityId });
    }
}
