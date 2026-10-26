namespace Ecommerce.Infrastructure.Persistence;

/// <summary>
/// Domain events written in the same transaction as the state change, then published by
/// <see cref="BackgroundJobs.OutboxPublisher"/>. This is what stops "order saved but email never sent".
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public OutboxMessage(string type, string payload, DateTimeOffset occurredAt)
    {
        Type = type;
        Payload = payload;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public string? Error { get; private set; }

    public void MarkProcessed(DateTimeOffset at) => ProcessedAt = at;

    public void MarkFailed(string error) => Error = error;
}
