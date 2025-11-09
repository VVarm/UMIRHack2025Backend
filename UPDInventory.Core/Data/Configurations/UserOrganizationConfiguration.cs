using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Data.Configurations
{
    public class UserOrganizationConfiguration : IEntityTypeConfiguration<UserOrganization>
    {
        public void Configure(EntityTypeBuilder<UserOrganization> builder)
        {
            builder.ToTable("user_organizations");

            // Primary Key
            builder.HasKey(uo => uo.Id);

            // Properties
            builder.Property(uo => uo.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(uo => uo.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(uo => uo.OrganizationId)
                .HasColumnName("organization_id")
                .IsRequired();

            builder.Property(uo => uo.Role)
                .HasColumnName("role")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(uo => uo.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Indexes
            builder.HasIndex(uo => new { uo.UserId, uo.OrganizationId })
                .IsUnique();

            // Check Constraint
            builder.HasCheckConstraint("CK_UserOrganizations_Role", "role IN ('storekeeper', 'auditor', 'admin', 'owner')");

            // Relationships
            builder.HasOne(uo => uo.User)
                .WithMany(u => u.UserOrganizations)
                .HasForeignKey(uo => uo.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uo => uo.Organization)
                .WithMany(o => o.UserOrganizations)
                .HasForeignKey(uo => uo.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}