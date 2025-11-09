using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(p => p.Barcode)
                .HasColumnName("barcode")
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasColumnName("description");

            builder.Property(p => p.OrganizationId)
                .HasColumnName("organization_id")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Indexes - уникальность barcode в рамках организации
            builder.HasIndex(p => new { p.OrganizationId, p.Barcode })
                .IsUnique();

            // Relationships
            builder.HasOne(p => p.Organization)
                .WithMany(o => o.Products)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.DocumentItems)
                .WithOne(di => di.Product)
                .HasForeignKey(di => di.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}