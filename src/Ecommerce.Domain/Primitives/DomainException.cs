namespace Ecommerce.Domain.Primitives;

/// <summary>Thrown when an invariant of the domain is violated. Handlers translate this into a validation error.</summary>
public sealed class DomainException(string message) : Exception(message);
