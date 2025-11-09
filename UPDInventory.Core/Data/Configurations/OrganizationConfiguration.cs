using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Data.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("organizations");

            // Primary Key
            builder.HasKey(o => o.Id);

            // Properties
            builder.Property(o => o.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(o => o.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(o => o.Inn)
                .HasColumnName("inn")
                .HasMaxLength(20);

            builder.Property(o => o.Address)
                .HasColumnName("address");

            builder.Property(o => o.Phone)
                .HasColumnName("phone")
                .HasMaxLength(50);

            builder.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Indexes
            builder.HasIndex(o => o.Inn)
                .IsUnique();

            // Relationships
            builder.HasMany(o => o.UserOrganizations)
                .WithOne(uo => uo.Organization)
                .HasForeignKey(uo => uo.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Warehouses)
                .WithOne(w => w.Organization)
                .HasForeignKey(w => w.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Products)
                .WithOne(p => p.Organization)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Documents)
                .WithOne(d => d.Organization)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.MobileSessions)
                .WithOne(ms => ms.Organization)
                .HasForeignKey(ms => ms.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}