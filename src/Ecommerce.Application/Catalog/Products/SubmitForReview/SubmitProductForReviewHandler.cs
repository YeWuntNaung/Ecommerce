using Ecommerce.Application.Abstractions;
using Ecommerce.Domain.Primitives;

namespace Ecommerce.Application.Catalog.Products.SubmitForReview;

public sealed record SubmitProductForReviewCommand(Guid ProductId);

public sealed class SubmitProductForReviewHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result> Handle(SubmitProductForReviewCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Fail(Error.Unauthorized());
        }

        var vendor = await vendors.GetByOwnerUserIdAsync(userId, cancellationToken);

        if (vendor is null)
        {
            return Result.Fail(Error.Forbidden("Your account is not linked to a vendor storefront."));
        }

        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null || product.VendorId != vendor.Id)
        {
            return Result.Fail(Error.NotFound("Product not found."));
        }

        try
        {
            product.SubmitForReview(clock.UtcNow);
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Conflict("invalid_product_state", exception.Message));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
