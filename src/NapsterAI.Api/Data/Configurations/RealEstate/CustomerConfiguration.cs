using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "dbo");
        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerId).HasColumnName("CustomerID");

        builder.Property(c => c.FullName).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Nationality).HasMaxLength(100);
        builder.Property(c => c.PreferredLanguage).HasMaxLength(50);
        builder.Property(c => c.Source).HasMaxLength(30);

        builder.Property(c => c.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(c => c.Email).IsUnique();
    }
}
