using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Catalog.Products.Review;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Primitives;
using NSubstitute;
using Shouldly;

namespace Ecommerce.Application.Tests.Catalog;

public sealed class ApproveProductHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly Guid ReviewerId = Guid.CreateVersion7();

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly Product _product = CreatePendingProduct();

    public ApproveProductHandlerTests()
    {
        _currentUser.UserId.Returns(ReviewerId);
        _currentUser.IsInRole("Admin").Returns(true);
        _products.GetByIdAsync(_product.Id, Arg.Any<CancellationToken>()).Returns(_product);
        _clock.UtcNow.Returns(Now);
    }

    [Fact]
    public async Task Admin_approves_a_pending_product()
    {
        var result = await CreateHandler().Handle(new ApproveProductCommand(_product.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _product.Status.ShouldBe(ProductStatus.Approved);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Non_admin_is_forbidden()
    {
        _currentUser.IsInRole("Admin").Returns(false);

        var result = await CreateHandler().Handle(new ApproveProductCommand(_product.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Forbidden);
        _product.Status.ShouldBe(ProductStatus.PendingReview);
    }

    [Fact]
    public async Task Missing_product_returns_not_found()
    {
        var result = await CreateHandler().Handle(new ApproveProductCommand(Guid.CreateVersion7()), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task Non_pending_product_returns_conflict()
    {
        _product.Approve(ReviewerId, Now);

        var result = await CreateHandler().Handle(new ApproveProductCommand(_product.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Conflict);
    }

    private ApproveProductHandler CreateHandler() =>
        new(_currentUser, _products, _unitOfWork, _clock);

    private static Product CreatePendingProduct()
    {
        var product = Product.CreateDraft(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Test Product",
            "test-product",
            "A description.",
            Now);
        product.AddVariant("Default", "SKU-1", Money.Kyat(1_000), Weight.FromGrams(100), 5, Now);
        product.SubmitForReview(Now);
        return product;
    }
}
