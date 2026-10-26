using System.Text;

namespace Ecommerce.Domain.Primitives;

public static class SlugGenerator
{
    private const int MaxLength = 60;

    /// <summary>Builds a URL-safe slug. Non-Latin input (e.g. Myanmar script) is transliteration-free,
    /// so it yields an empty string and callers must supply a fallback.</summary>
    public static string From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        var lastWasSeparator = true;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(character);
                lastWasSeparator = false;
            }
            else if (!lastWasSeparator)
            {
                builder.Append('-');
                lastWasSeparator = true;
            }
        }

        var slug = builder.ToString().Trim('-');
        return slug.Length > MaxLength ? slug[..MaxLength].Trim('-') : slug;
    }

    public static string FromWithFallback(string? value, string prefix)
    {
        var slug = From(value);
        return slug.Length > 0 ? slug : $"{prefix}-{Guid.CreateVersion7().ToString("N")[..8]}";
    }
}
