using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            // Primary Key
            builder.HasKey(u => u.Id);

            // Properties
            builder.Property(u => u.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Indexes
            builder.HasIndex(u => u.Email)
                .IsUnique();

            // Relationships
            builder.HasMany(u => u.UserOrganizations)
                .WithOne(uo => uo.User)
                .HasForeignKey(uo => uo.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Documents)
                .WithOne(d => d.CreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.MobileSessions)
                .WithOne(ms => ms.CreatedBy)
                .HasForeignKey(ms => ms.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}