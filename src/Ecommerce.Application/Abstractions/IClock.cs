namespace Ecommerce.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
