using Ecommerce.Domain.Primitives;

namespace Ecommerce.Domain.Catalog;

public sealed class Product : AggregateRoot
{
    private Product()
    {
    }

    public Guid VendorId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ProductStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public string? RejectionReason { get; private set; }

    public ICollection<ProductVariant> Variants { get; private set; } = new List<ProductVariant>();

    public static Product CreateDraft(
        Guid vendorId,
        Guid categoryId,
        string name,
        string slug,
        string description,
        DateTimeOffset now)
    {
        if (vendorId == Guid.Empty)
        {
            throw new DomainException("A product must belong to a vendor.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("A product must belong to a category.");
        }

        var trimmedName = name?.Trim() ?? string.Empty;

        if (trimmedName.Length is < 3 or > 200)
        {
            throw new DomainException("Product name must be between 3 and 200 characters.");
        }

        if (description?.Length > 4000)
        {
            throw new DomainException("Product description must be at most 4000 characters.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("A product slug is required.");
        }

        return new Product
        {
            VendorId = vendorId,
            CategoryId = categoryId,
            Name = trimmedName,
            Slug = slug,
            Description = description?.Trim() ?? string.Empty,
            Status = ProductStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public ProductVariant AddVariant(string name, string sku, Money price, Weight weight, int stockOnHand, DateTimeOffset now)
    {
        EnsureEditable();

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
        {
            throw new DomainException("Variant name is required (max 120 characters).");
        }

        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 64)
        {
            throw new DomainException("Variant SKU is required (max 64 characters).");
        }

        if (price.Amount <= 0)
        {
            throw new DomainException("Variant price must be greater than zero.");
        }

        if (weight.Grams <= 0)
        {
            throw new DomainException("Variant weight must be greater than zero.");
        }

        if (stockOnHand < 0)
        {
            throw new DomainException("Stock cannot be negative.");
        }

        var normalizedSku = sku.Trim().ToUpperInvariant();

        if (Variants.Any(variant => variant.Sku == normalizedSku))
        {
            throw new DomainException($"SKU '{normalizedSku}' already exists on this product.");
        }

        var created = new ProductVariant(Id, name.Trim(), normalizedSku, price.Amount, weight.Grams, stockOnHand);
        Variants.Add(created);
        UpdatedAt = now;
        return created;
    }

    public void UpdateDetails(string name, string description, DateTimeOffset now)
    {
        EnsureEditable();

        var trimmedName = name?.Trim() ?? string.Empty;

        if (trimmedName.Length is < 3 or > 200)
        {
            throw new DomainException("Product name must be between 3 and 200 characters.");
        }

        Name = trimmedName;
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = now;
    }

    public void SubmitForReview(DateTimeOffset now)
    {
        if (Status is not (ProductStatus.Draft or ProductStatus.Rejected))
        {
            throw new DomainException("Only a draft or rejected product can be submitted for review.");
        }

        if (Variants.Count == 0)
        {
            throw new DomainException("Add at least one variant before submitting for review.");
        }

        Status = ProductStatus.PendingReview;
        SubmittedAt = now;
        UpdatedAt = now;
        RejectionReason = null;
        Raise(new ProductSubmittedForReview(Id, VendorId, now));
    }

    public void Approve(Guid reviewerUserId, DateTimeOffset now)
    {
        if (Status != ProductStatus.PendingReview)
        {
            throw new DomainException("Only a product under review can be approved.");
        }

        Status = ProductStatus.Approved;
        ReviewedByUserId = reviewerUserId;
        ReviewedAt = now;
        UpdatedAt = now;
        Raise(new ProductApproved(Id, VendorId, now));
    }

    public void Reject(Guid reviewerUserId, string reason, DateTimeOffset now)
    {
        if (Status != ProductStatus.PendingReview)
        {
            throw new DomainException("Only a product under review can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("A rejection reason is required.");
        }

        Status = ProductStatus.Rejected;
        ReviewedByUserId = reviewerUserId;
        ReviewedAt = now;
        UpdatedAt = now;
        RejectionReason = reason.Trim();
        Raise(new ProductRejected(Id, VendorId, reason.Trim(), now));
    }

    public void Suspend(string reason, DateTimeOffset now)
    {
        if (Status != ProductStatus.Approved)
        {
            throw new DomainException("Only an approved product can be suspended.");
        }

        Status = ProductStatus.Suspended;
        UpdatedAt = now;
        RejectionReason = reason?.Trim();
    }

    private void EnsureEditable()
    {
        if (Status == ProductStatus.PendingReview)
        {
            throw new DomainException("The product is under review and cannot be edited until a decision is made.");
        }
    }
}
