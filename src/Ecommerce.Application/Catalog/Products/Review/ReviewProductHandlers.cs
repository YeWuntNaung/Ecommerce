using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Common;
using Ecommerce.Domain.Primitives;
using FluentValidation;

namespace Ecommerce.Application.Catalog.Products.Review;

public sealed record ApproveProductCommand(Guid ProductId);

public sealed record RejectProductCommand(Guid ProductId, string Reason);

public sealed class RejectProductCommandValidator : AbstractValidator<RejectProductCommand>
{
    public RejectProductCommandValidator()
    {
        RuleFor(command => command.Reason)
            .NotEmpty().WithMessage("A rejection reason is required.")
            .MaximumLength(1000);
    }
}

public sealed class ApproveProductHandler(
    ICurrentUser currentUser,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result> Handle(ApproveProductCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole("Admin"))
        {
            return Result.Fail(Error.Forbidden());
        }

        if (currentUser.UserId is not { } reviewerId)
        {
            return Result.Fail(Error.Unauthorized());
        }

        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Fail(Error.NotFound("Product not found."));
        }

        try
        {
            product.Approve(reviewerId, clock.UtcNow);
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Conflict("invalid_product_state", exception.Message));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed class RejectProductHandler(
    ICurrentUser currentUser,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IValidator<RejectProductCommand> validator,
    IClock clock)
{
    public async Task<Result> Handle(RejectProductCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);

        if (!validation.IsValid)
        {
            return Result.Fail(validation.ToError());
        }

        if (!currentUser.IsInRole("Admin"))
        {
            return Result.Fail(Error.Forbidden());
        }

        if (currentUser.UserId is not { } reviewerId)
        {
            return Result.Fail(Error.Unauthorized());
        }

        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Fail(Error.NotFound("Product not found."));
        }

        try
        {
            product.Reject(reviewerId, command.Reason, clock.UtcNow);
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Conflict("invalid_product_state", exception.Message));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
