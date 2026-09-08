using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Data.Configurations.RealEstate;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "dbo");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).HasColumnName("UserID");
        builder.Property(u => u.SalesAgentId).HasColumnName("SalesAgentID");

        builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(300).IsRequired();
        builder.Property(u => u.Role).HasMaxLength(30).IsRequired();
        builder.Property(u => u.RefreshToken).HasMaxLength(200);

        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.SalesAgentId);

        builder.HasOne(u => u.SalesAgent)
            .WithMany()
            .HasForeignKey(u => u.SalesAgentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
