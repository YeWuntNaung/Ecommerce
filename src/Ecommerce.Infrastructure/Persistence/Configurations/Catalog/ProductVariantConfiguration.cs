using Ecommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants", "catalog");
        builder.HasKey(variant => variant.Id);

        builder.Property(variant => variant.Name).HasMaxLength(120).IsRequired();
        builder.Property(variant => variant.Sku).HasMaxLength(64).IsRequired();

        builder.HasIndex(variant => new { variant.ProductId, variant.Sku }).IsUnique();

        builder.Ignore(variant => variant.Price);
        builder.Ignore(variant => variant.Weight);
    }
}
