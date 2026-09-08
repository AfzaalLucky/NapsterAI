using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class LookupConfiguration : IEntityTypeConfiguration<Lookup>
{
    public void Configure(EntityTypeBuilder<Lookup> builder)
    {
        builder.ToTable("Lookups", "dbo");
        builder.HasKey(l => l.LookupId);
        builder.Property(l => l.LookupId).HasColumnName("LookupID");

        builder.Property(l => l.LookupType).HasMaxLength(50).IsRequired();
        builder.Property(l => l.Code).HasMaxLength(50).IsRequired();
        builder.Property(l => l.DisplayName).HasMaxLength(150).IsRequired();

        builder.HasIndex(l => new { l.LookupType, l.Code }).IsUnique();
    }
}
