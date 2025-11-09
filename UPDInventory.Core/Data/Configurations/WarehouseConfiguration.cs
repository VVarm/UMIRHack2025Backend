using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Data.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("warehouses");

            // Primary Key
            builder.HasKey(w => w.Id);

            // Properties
            builder.Property(w => w.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(w => w.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(w => w.Address)
                .HasColumnName("address");

            builder.Property(w => w.OrganizationId)
                .HasColumnName("organization_id")
                .IsRequired();

            builder.Property(w => w.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(w => w.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Relationships
            builder.HasOne(w => w.Organization)
                .WithMany(o => o.Warehouses)
                .HasForeignKey(w => w.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(w => w.Documents)
                .WithOne(d => d.Warehouse)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}