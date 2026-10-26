namespace Ecommerce.Domain.Primitives;

/// <summary>Marker for something that happened in the domain and must be published reliably (via the outbox).</summary>
public interface IDomainEvent;
