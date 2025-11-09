using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Data.Configurations
{
    public class MobileSessionConfiguration : IEntityTypeConfiguration<MobileSession>
    {
        public void Configure(EntityTypeBuilder<MobileSession> builder)
        {
            builder.ToTable("mobile_sessions");

            // Primary Key
            builder.HasKey(ms => ms.Id);

            // Properties
            builder.Property(ms => ms.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(ms => ms.Token)
                .HasColumnName("token")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(ms => ms.OrganizationId)
                .HasColumnName("organization_id")
                .IsRequired();

            builder.Property(ms => ms.CreatedById)
                .HasColumnName("created_by_id")
                .IsRequired();

            builder.Property(ms => ms.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            builder.Property(ms => ms.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            builder.Property(ms => ms.IsUsed)
                .HasColumnName("is_used")
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(ms => ms.Token)
                .IsUnique();

            builder.HasIndex(ms => ms.ExpiresAt);

            // Relationships
            builder.HasOne(ms => ms.Organization)
                .WithMany(o => o.MobileSessions)
                .HasForeignKey(ms => ms.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ms => ms.CreatedBy)
                .WithMany(u => u.MobileSessions)
                .HasForeignKey(ms => ms.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}