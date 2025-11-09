using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Data.Configurations
{
    public class DocumentItemConfiguration : IEntityTypeConfiguration<DocumentItem>
    {
        public void Configure(EntityTypeBuilder<DocumentItem> builder)
        {
            builder.ToTable("DocumentItems");

            builder.HasKey(di => di.Id);

            builder.Property(di => di.QuantityExpected)
                .IsRequired()
                .HasColumnType("decimal(18,3)");

            builder.Property(di => di.QuantityActual)
                .IsRequired()
                .HasColumnType("decimal(18,3)");

            // Связи
            builder.HasOne(di => di.Document)
                .WithMany(d => d.Items)
                .HasForeignKey(di => di.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(di => di.Product)
                .WithMany(p => p.DocumentItems)
                .HasForeignKey(di => di.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            builder.HasIndex(di => di.DocumentId);
            builder.HasIndex(di => di.ProductId);
        }
    }
}