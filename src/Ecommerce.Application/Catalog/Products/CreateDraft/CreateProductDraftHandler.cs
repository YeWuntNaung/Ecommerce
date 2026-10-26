using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Common;
using Ecommerce.Contracts.Catalog;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Primitives;
using FluentValidation;

namespace Ecommerce.Application.Catalog.Products.CreateDraft;

public sealed record CreateProductVariantInput(
    string Name,
    string Sku,
    long PriceKyat,
    int WeightGrams,
    int StockOnHand);

public sealed record CreateProductDraftCommand(
    Guid CategoryId,
    string Name,
    string Description,
    IReadOnlyList<CreateProductVariantInput> Variants);

public sealed class CreateProductDraftCommandValidator : AbstractValidator<CreateProductDraftCommand>
{
    public CreateProductDraftCommandValidator()
    {
        RuleFor(command => command.CategoryId).NotEmpty().WithMessage("Choose a category.");
        RuleFor(command => command.Name).NotEmpty().Length(3, 200);
        RuleFor(command => command.Description).MaximumLength(4000);
        RuleFor(command => command.Variants).NotEmpty().WithMessage("Add at least one variant.");

        RuleForEach(command => command.Variants).ChildRules(variant =>
        {
            variant.RuleFor(v => v.Name).NotEmpty().MaximumLength(120);
            variant.RuleFor(v => v.Sku).NotEmpty().MaximumLength(64);
            variant.RuleFor(v => v.PriceKyat).GreaterThan(0).WithMessage("Price must be greater than zero.");
            variant.RuleFor(v => v.WeightGrams).GreaterThan(0).WithMessage("Weight must be greater than zero.");
            variant.RuleFor(v => v.StockOnHand).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class CreateProductDraftHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IProductRepository products,
    ICatalogQueries catalogQueries,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductDraftCommand> validator,
    IClock clock)
{
    public async Task<Result<CreateProductDraftResponse>> Handle(
        CreateProductDraftCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);

        if (!validation.IsValid)
        {
            return Result.Fail<CreateProductDraftResponse>(validation.ToError());
        }

        if (currentUser.UserId is not { } userId)
        {
            return Result.Fail<CreateProductDraftResponse>(Error.Unauthorized());
        }

        var vendor = await vendors.GetByOwnerUserIdAsync(userId, cancellationToken);

        if (vendor is null)
        {
            return Result.Fail<CreateProductDraftResponse>(
                Error.Forbidden("Your account is not linked to a vendor storefront."));
        }

        if (!vendor.CanSell)
        {
            return Result.Fail<CreateProductDraftResponse>(
                Error.Conflict("vendor_not_active", "Your storefront must be approved before you can add products."));
        }

        if (!await catalogQueries.CategoryExistsAsync(command.CategoryId, cancellationToken))
        {
            return Result.Fail<CreateProductDraftResponse>(Error.NotFound("Category not found."));
        }

        var baseSlug = SlugGenerator.FromWithFallback(command.Name, "product");
        var slug = baseSlug;
        var suffix = 2;

        while (await products.SlugExistsAsync(vendor.Id, slug, cancellationToken))
        {
            if (suffix > 50)
            {
                return Result.Fail<CreateProductDraftResponse>(
                    Error.Conflict("slug_taken", "Could not allocate a unique product slug. Change the product name."));
            }

            slug = $"{baseSlug}-{suffix++}";
        }

        try
        {
            var now = clock.UtcNow;
            var product = Product.CreateDraft(
                vendor.Id,
                command.CategoryId,
                command.Name,
                slug,
                command.Description,
                now);

            foreach (var input in command.Variants)
            {
                product.AddVariant(
                    input.Name,
                    input.Sku,
                    Money.Kyat(input.PriceKyat),
                    Weight.FromGrams(input.WeightGrams),
                    input.StockOnHand,
                    now);
            }

            await products.AddAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok(new CreateProductDraftResponse(product.Id, product.Slug));
        }
        catch (DomainException exception)
        {
            return Result.Fail<CreateProductDraftResponse>(Error.Validation("product", exception.Message));
        }
    }
}
