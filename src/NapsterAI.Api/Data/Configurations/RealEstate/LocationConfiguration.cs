using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations", "dbo");
        builder.HasKey(l => l.LocationId);
        builder.Property(l => l.LocationId).HasColumnName("LocationID");

        builder.Property(l => l.Country).HasMaxLength(100).IsRequired();
        builder.Property(l => l.City).HasMaxLength(100).IsRequired();
        builder.Property(l => l.District).HasMaxLength(150);
        builder.Property(l => l.Latitude).HasColumnType("decimal(9,6)");
        builder.Property(l => l.Longitude).HasColumnType("decimal(9,6)");

        builder.HasIndex(l => new { l.Country, l.City, l.District }).IsUnique();
    }
}
