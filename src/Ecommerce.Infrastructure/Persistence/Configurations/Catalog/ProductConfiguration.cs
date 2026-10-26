using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Vendors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", "catalog");
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Slug).HasMaxLength(60).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(4000);
        builder.Property(product => product.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(product => product.RejectionReason).HasMaxLength(1000);

        builder.HasIndex(product => new { product.VendorId, product.Slug }).IsUnique();
        builder.HasIndex(product => new { product.VendorId, product.Status });

        builder.HasOne<Vendor>()
            .WithMany()
            .HasForeignKey(product => product.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(product => product.Variants)
            .WithOne()
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(product => product.Variants)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
