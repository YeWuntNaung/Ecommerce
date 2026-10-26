using Ecommerce.Domain.Primitives;

namespace Ecommerce.Domain.Catalog;

public sealed record ProductSubmittedForReview(Guid ProductId, Guid VendorId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record ProductApproved(Guid ProductId, Guid VendorId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record ProductRejected(Guid ProductId, Guid VendorId, string Reason, DateTimeOffset OccurredAt) : IDomainEvent;
