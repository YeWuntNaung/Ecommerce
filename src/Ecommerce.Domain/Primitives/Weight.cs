namespace Ecommerce.Domain.Primitives;

/// <summary>
/// Parcel weight. Persisted as integer grams so shipping bands never suffer floating point drift;
/// presented to users in kilograms.
/// </summary>
public readonly record struct Weight
{
    public const int GramsPerKilogram = 1000;

    private Weight(int grams)
    {
        if (grams < 0)
        {
            throw new DomainException("Weight cannot be negative.");
        }

        Grams = grams;
    }

    public int Grams { get; }

    public decimal Kilograms => Grams / (decimal)GramsPerKilogram;

    public static Weight FromGrams(int grams) => new(grams);

    public static Weight FromKilograms(decimal kilograms) =>
        new((int)Math.Round(kilograms * GramsPerKilogram, 0, MidpointRounding.AwayFromZero));

    public static Weight operator +(Weight left, Weight right) => FromGrams(checked(left.Grams + right.Grams));

    public override string ToString() => $"{Kilograms:0.###} kg";
}
