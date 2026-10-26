using Ecommerce.Domain.Vendors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations.Vendors;

public sealed class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("vendors", "vendors");
        builder.HasKey(vendor => vendor.Id);

        builder.Property(vendor => vendor.Name).HasMaxLength(120).IsRequired();
        builder.Property(vendor => vendor.Description).HasMaxLength(2000);
        builder.Property(vendor => vendor.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(vendor => vendor.Slug)
            .HasConversion(slug => slug.Value, value => VendorSlug.FromTrusted(value))
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(vendor => vendor.Slug).IsUnique();
        builder.HasIndex(vendor => vendor.OwnerUserId).IsUnique();
    }
}
