using Ecommerce.Domain.Primitives;
using Ecommerce.Domain.Vendors;
using Shouldly;

namespace Ecommerce.Domain.Tests;

public sealed class VendorSlugTests
{
    [Theory]
    [InlineData("Demo Store", "demo-store")]
    [InlineData("  Demo   Store!! ", "demo-store")]
    [InlineData("ABC-123", "abc-123")]
    public void Normalizes_to_a_url_safe_slug(string input, string expected)
    {
        VendorSlug.Create(input).Value.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("checkout")]
    [InlineData("ab")]
    public void Rejects_reserved_and_too_short_slugs(string input)
    {
        VendorSlug.Create(input).IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void Empty_input_is_a_validation_error()
    {
        VendorSlug.Create("   ").Error!.Type.ShouldBe(ErrorType.Validation);
    }
}
