using Ecommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations.Catalog;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", "catalog");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name).HasMaxLength(120).IsRequired();
        builder.Property(category => category.Slug).HasMaxLength(60).IsRequired();

        builder.HasIndex(category => category.Slug).IsUnique();
    }
}
