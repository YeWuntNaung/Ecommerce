using Ecommerce.Domain.Primitives;

namespace Ecommerce.Domain.Vendors;

public sealed class Vendor : AggregateRoot
{
    private Vendor()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public VendorSlug Slug { get; private set; } = null!;

    public string Description { get; private set; } = string.Empty;

    public VendorStatus Status { get; private set; }

    /// <summary>The identity user that owns this storefront. Resolved from the authenticated principal.</summary>
    public Guid OwnerUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ApprovedAt { get; private set; }

    public bool CanSell => Status == VendorStatus.Active;

    public static Vendor Create(string name, VendorSlug slug, string description, Guid ownerUserId, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
        {
            throw new DomainException("Vendor name is required (max 120 characters).");
        }

        if (ownerUserId == Guid.Empty)
        {
            throw new DomainException("Vendor owner is required.");
        }

        return new Vendor
        {
            Name = name.Trim(),
            Slug = slug,
            Description = description?.Trim() ?? string.Empty,
            OwnerUserId = ownerUserId,
            Status = VendorStatus.Pending,
            CreatedAt = now,
        };
    }

    public void Approve(DateTimeOffset now)
    {
        if (Status == VendorStatus.Active)
        {
            throw new DomainException("Vendor is already active.");
        }

        Status = VendorStatus.Active;
        ApprovedAt = now;
    }

    public void Suspend()
    {
        if (Status != VendorStatus.Active)
        {
            throw new DomainException("Only an active vendor can be suspended.");
        }

        Status = VendorStatus.Suspended;
    }
}
