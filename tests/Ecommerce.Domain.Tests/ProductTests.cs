using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Primitives;
using Shouldly;

namespace Ecommerce.Domain.Tests;

public sealed class ProductTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Cannot_submit_without_variants()
    {
        var product = Product.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), "Test Product", "test-product", "desc", Now);

        var exception = Should.Throw<DomainException>(() => product.SubmitForReview(Now));
        exception.Message.ShouldContain("variant");
    }

    [Fact]
    public void Approval_requires_pending_review()
    {
        var product = CreateDraft();

        Should.Throw<DomainException>(() => product.Approve(Guid.NewGuid(), Now));
    }

    [Fact]
    public void Submit_then_approve_transitions_and_raises_events()
    {
        var product = CreatePending();
        product.Status.ShouldBe(ProductStatus.PendingReview);
        product.SubmittedAt.ShouldBe(Now);

        var reviewer = Guid.CreateVersion7();
        product.Approve(reviewer, Now);

        product.Status.ShouldBe(ProductStatus.Approved);
        product.ReviewedByUserId.ShouldBe(reviewer);
        product.DomainEvents.ShouldContain(domainEvent => domainEvent is ProductApproved);
    }

    [Fact]
    public void Rejection_requires_a_reason()
    {
        var product = CreatePending();

        Should.Throw<DomainException>(() => product.Reject(Guid.NewGuid(), "  ", Now));
    }

    [Fact]
    public void Rejected_product_can_be_resubmitted()
    {
        var product = CreatePending();
        product.Reject(Guid.NewGuid(), "Photos are unclear.", Now);

        product.SubmitForReview(Now);

        product.Status.ShouldBe(ProductStatus.PendingReview);
        product.RejectionReason.ShouldBeNull();
    }

    [Fact]
    public void Cannot_edit_while_under_review()
    {
        var product = CreatePending();

        Should.Throw<DomainException>(() =>
            product.AddVariant("New", "SKU-NEW", Money.Kyat(100), Weight.FromGrams(10), 1, Now));
    }

    [Fact]
    public void Rejects_duplicate_sku_and_non_positive_price()
    {
        var product = CreateDraft();
        product.AddVariant("Default", "SKU-1", Money.Kyat(1_000), Weight.FromGrams(100), 1, Now);

        Should.Throw<DomainException>(() =>
            product.AddVariant("Second", "sku-1", Money.Kyat(1_000), Weight.FromGrams(100), 1, Now));

        Should.Throw<DomainException>(() =>
            product.AddVariant("Free", "SKU-2", Money.Kyat(0), Weight.FromGrams(100), 1, Now));
    }

    private static Product CreateDraft() =>
        Product.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), "Test Product", "test-product", "A description.", Now);

    private static Product CreatePending()
    {
        var product = CreateDraft();
        product.AddVariant("Default", "SKU-1", Money.Kyat(1_000), Weight.FromGrams(100), 5, Now);
        product.SubmitForReview(Now);
        return product;
    }
}
