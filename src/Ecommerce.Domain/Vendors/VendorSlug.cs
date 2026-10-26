using Ecommerce.Domain.Primitives;

namespace Ecommerce.Domain.Vendors;

/// <summary>The public storefront identifier, e.g. <c>/store/demo-store</c>.</summary>
public sealed record VendorSlug
{
    private static readonly HashSet<string> Reserved = new(StringComparer.Ordinal)
    {
        "admin", "api", "app", "account", "cart", "checkout", "login", "logout",
        "payment", "payments", "register", "store", "stores", "support", "vendor", "www",
    };

    private VendorSlug(string value) => Value = value;

    public string Value { get; }

    public static Result<VendorSlug> Create(string? raw)
    {
        var slug = SlugGenerator.From(raw);

        if (slug.Length < 3)
        {
            return Result.Fail<VendorSlug>(Error.Validation("slug", "Storefront slug must be at least 3 characters (a-z, 0-9, hyphen)."));
        }

        if (slug.Length > 40)
        {
            return Result.Fail<VendorSlug>(Error.Validation("slug", "Storefront slug must be at most 40 characters."));
        }

        if (Reserved.Contains(slug))
        {
            return Result.Fail<VendorSlug>(Error.Validation("slug", "That storefront slug is reserved."));
        }

        return Result.Ok(new VendorSlug(slug));
    }

    /// <summary>Used by persistence when rehydrating a trusted value.</summary>
    public static VendorSlug FromTrusted(string value) => new(value);

    public override string ToString() => Value;
}
