using Ecommerce.Domain.Primitives;

namespace Ecommerce.Domain.Catalog;

public sealed class ProductVariant : Entity
{
    private ProductVariant()
    {
    }

    internal ProductVariant(Guid productId, string name, string sku, long priceKyat, int weightGrams, int stockOnHand)
    {
        ProductId = productId;
        Name = name;
        Sku = sku;
        PriceKyat = priceKyat;
        WeightGrams = weightGrams;
        StockOnHand = stockOnHand;
        IsActive = true;
    }

    public Guid ProductId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Sku { get; private set; } = string.Empty;

    public long PriceKyat { get; private set; }

    public int WeightGrams { get; private set; }

    public int StockOnHand { get; private set; }

    public bool IsActive { get; private set; }

    public Money Price => Money.Kyat(PriceKyat);

    public Weight Weight => Weight.FromGrams(WeightGrams);
}
