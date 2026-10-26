using Ecommerce.Domain.Primitives;
using Shouldly;

namespace Ecommerce.Domain.Tests;

public sealed class WeightTests
{
    [Fact]
    public void Converts_kilograms_to_grams()
    {
        Weight.FromKilograms(1.5m).Grams.ShouldBe(1_500);
        Weight.FromKilograms(0.999m).Grams.ShouldBe(999);
        Weight.FromKilograms(0.001m).Grams.ShouldBe(1);
    }

    [Fact]
    public void Shipping_band_boundary_is_representable()
    {
        var justUnderOneKg = Weight.FromGrams(999);
        var exactlyOneKg = Weight.FromGrams(1_000);

        justUnderOneKg.Kilograms.ShouldBe(0.999m);
        exactlyOneKg.Kilograms.ShouldBe(1m);
        (justUnderOneKg + Weight.FromGrams(1)).Grams.ShouldBe(exactlyOneKg.Grams);
    }

    [Fact]
    public void Rejects_negative_weight()
    {
        Should.Throw<DomainException>(() => Weight.FromGrams(-1));
    }
}
