using Ecommerce.Application.Abstractions;

namespace Ecommerce.Application.Common;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
