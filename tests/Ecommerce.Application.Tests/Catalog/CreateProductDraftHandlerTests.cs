using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Catalog.Products.CreateDraft;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Primitives;
using Ecommerce.Domain.Vendors;
using NSubstitute;
using Shouldly;

namespace Ecommerce.Application.Tests.Catalog;

public sealed class CreateProductDraftHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly Guid VendorOwnerId = Guid.CreateVersion7();
    private static readonly Guid CategoryId = Guid.CreateVersion7();

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IVendorRepository _vendors = Substitute.For<IVendorRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly ICatalogQueries _queries = Substitute.For<ICatalogQueries>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();

    private readonly Vendor _vendor = CreateActiveVendor();

    public CreateProductDraftHandlerTests()
    {
        _currentUser.UserId.Returns(VendorOwnerId);
        _vendors.GetByOwnerUserIdAsync(VendorOwnerId, Arg.Any<CancellationToken>()).Returns(_vendor);
        _queries.CategoryExistsAsync(CategoryId, Arg.Any<CancellationToken>()).Returns(true);
        _clock.UtcNow.Returns(Now);
    }

    [Fact]
    public async Task Creates_a_draft_with_a_unique_slug()
    {
        Product? added = null;
        _products.AddAsync(Arg.Do<Product>(product => added = product), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand("Wireless Earbuds"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Slug.ShouldBe("wireless-earbuds");
        added.ShouldNotBeNull();
        added!.Status.ShouldBe(ProductStatus.Draft);
        added.Variants.Count.ShouldBe(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Appends_a_suffix_when_the_slug_is_taken()
    {
        _products.SlugExistsAsync(Arg.Any<Guid>(), "wireless-earbuds", Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().Handle(ValidCommand("Wireless Earbuds"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Slug.ShouldBe("wireless-earbuds-2");
    }

    [Fact]
    public async Task Rejects_unauthenticated_callers()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var result = await CreateHandler().Handle(ValidCommand("Wireless Earbuds"), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Rejects_vendors_that_are_not_approved()
    {
        var pendingVendor = Vendor.Create(
            "Pending Store",
            VendorSlug.Create("pending-store").Value,
            "not yet approved",
            VendorOwnerId,
            Now);
        _vendors.GetByOwnerUserIdAsync(VendorOwnerId, Arg.Any<CancellationToken>()).Returns(pendingVendor);

        var result = await CreateHandler().Handle(ValidCommand("Wireless Earbuds"), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public async Task Surfaces_validation_errors_from_the_command()
    {
        var command = new CreateProductDraftCommand(CategoryId, "Ok name", "desc", Array.Empty<CreateProductVariantInput>());

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Validation);
        result.Error.ValidationErrors!.ShouldContainKey("variants");
    }

    private CreateProductDraftHandler CreateHandler() =>
        new(_currentUser, _vendors, _products, _queries, _unitOfWork, new CreateProductDraftCommandValidator(), _clock);

    private static CreateProductDraftCommand ValidCommand(string name) =>
        new(
            CategoryId,
            name,
            "Compact Bluetooth earbuds.",
            [new CreateProductVariantInput("Black", "EAR-BLK", 85_000, 200, 25)]);

    private static Vendor CreateActiveVendor()
    {
        var vendor = Vendor.Create(
            "Demo Store",
            VendorSlug.Create("demo-store").Value,
            "A demo storefront.",
            VendorOwnerId,
            Now);
        vendor.Approve(Now);
        return vendor;
    }
}
