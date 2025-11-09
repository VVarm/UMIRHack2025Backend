using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Enums;

namespace UPDInventory.Core.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(d => d.Number)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("draft");

            builder.Property(d => d.Comment)
                .HasMaxLength(1000);

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.DocumentDate)
                .IsRequired();

            // Связи
            builder.HasOne(d => d.Organization)
                .WithMany(o => o.Documents)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связи со складами - явно указываем навигационные свойства
            builder.HasOne(d => d.SourceWarehouse)
                .WithMany()
                .HasForeignKey(d => d.SourceWarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(d => d.DestinationWarehouse)
                .WithMany()
                .HasForeignKey(d => d.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(d => d.Warehouse)
                .WithMany(w => w.Documents)  // Указываем обратную навигацию
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(d => d.CreatedBy)
                .WithMany(u => u.Documents)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Навигационное свойство Items
            builder.HasMany(d => d.Items)
                .WithOne(di => di.Document)
                .HasForeignKey(di => di.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы
            builder.HasIndex(d => new { d.OrganizationId, d.Number })
                .IsUnique();

            builder.HasIndex(d => d.DocumentDate);
            builder.HasIndex(d => d.Type);
            builder.HasIndex(d => d.Status);
        }
    }
}