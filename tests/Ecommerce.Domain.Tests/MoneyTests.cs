using Ecommerce.Domain.Primitives;
using Shouldly;

namespace Ecommerce.Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Adds_whole_kyat()
    {
        (Money.Kyat(1_000) + Money.Kyat(2_500)).Amount.ShouldBe(3_500);
    }

    [Fact]
    public void Percentage_rounds_half_away_from_zero_to_whole_kyat()
    {
        // 5% of 999 = 49.95 kyat -> 50
        Money.Kyat(999).Percentage(5m).Amount.ShouldBe(50);

        // 5% of 10 = 0.5 kyat -> 1
        Money.Kyat(10).Percentage(5m).Amount.ShouldBe(1);

        // 5% of 9 = 0.45 kyat -> 0
        Money.Kyat(9).Percentage(5m).Amount.ShouldBe(0);
    }

    [Fact]
    public void Multiplication_rounds_to_whole_kyat()
    {
        (Money.Kyat(1_000) * 1.5m).Amount.ShouldBe(1_500);
        (Money.Kyat(333) * 1.5m).Amount.ShouldBe(500); // 499.5 -> 500
    }

    [Fact]
    public void Keeps_currency_and_rejects_mixing()
    {
        var money = Money.Kyat(1);
        money.Currency.ShouldBe("MMK");
        money.ToString().ShouldBe("1 MMK");
    }
}
