namespace Ecommerce.Contracts.Common;

public static class CurrencyCodes
{
    public const string Mmk = "MMK";
}

/// <summary>Transport shape for money. Always whole kyat for this marketplace.</summary>
public sealed record MoneyDto(long Amount, string Currency)
{
    public static MoneyDto Kyat(long amount) => new(amount, CurrencyCodes.Mmk);
}
