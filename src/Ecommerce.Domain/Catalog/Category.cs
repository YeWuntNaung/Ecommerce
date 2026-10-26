using Ecommerce.Domain.Primitives;

namespace Ecommerce.Domain.Catalog;

public sealed class Category : Entity
{
    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public Guid? ParentId { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; } = true;

    public static Category Create(string name, string slug, int sortOrder = 0, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
        {
            throw new DomainException("Category name is required (max 120 characters).");
        }

        return new Category
        {
            Name = name.Trim(),
            Slug = SlugGenerator.FromWithFallback(slug, "category"),
            ParentId = parentId,
            SortOrder = sortOrder,
            IsActive = true,
        };
    }
}
