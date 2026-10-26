namespace Ecommerce.Domain.Primitives;

/// <summary>
/// Money in whole kyat. The marketplace is MMK-only by decision, and MMK has no circulating subunit,
/// so amounts are stored as <see cref="long"/> kyat and every rounding decision happens in one place.
/// </summary>
public sealed class Money : IEquatable<Money>, IComparable<Money>
{
    public const string Mmk = "MMK";

    private Money(long amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public long Amount { get; }

    public string Currency { get; }

    public static Money Kyat(long amount) => new(amount, Mmk);

    public static Money Zero => Kyat(0);

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal multiplier) =>
        new(RoundToKyat(money.Amount * multiplier), money.Currency);

    /// <summary>Applies a percentage (e.g. 5 for 5%) with the single documented rounding rule.</summary>
    public Money Percentage(decimal percent) => new(RoundToKyat(Amount * percent / 100m), Currency);

    /// <summary>Rounds half away from zero to the nearest kyat. The only rounding rule in the system.</summary>
    public static long RoundToKyat(decimal value) => (long)Math.Round(value, 0, MidpointRounding.AwayFromZero);

    public bool Equals(Money? other) =>
        other is not null && other.Amount == Amount && other.Currency == Currency;

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public int CompareTo(Money? other) => other is null
        ? 1
        : string.Equals(Currency, other.Currency, StringComparison.Ordinal)
            ? Amount.CompareTo(other.Amount)
            : throw new DomainException("Cannot compare money in different currencies.");

    public override string ToString() => $"{Amount:N0} {Currency}";

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);

    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
        {
            throw new DomainException("Cannot combine money in different currencies.");
        }
    }
}
