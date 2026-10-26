using Ecommerce.Api.Common;
using Ecommerce.Application.Catalog.Products.CreateDraft;
using Ecommerce.Application.Catalog.Products.SubmitForReview;
using Ecommerce.Application.Vendors;
using Ecommerce.Contracts.Catalog;

namespace Ecommerce.Api.Endpoints;

public static class VendorProductEndpoints
{
    public static IEndpointRouteBuilder MapVendorProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/vendor")
            .WithTags("Vendor")
            .RequireAuthorization("VendorOwner");

        group.MapGet("/me", async (GetMyVendorHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(cancellationToken);
                return result.ToHttpResult(value => Results.Ok(value));
            })
            .WithName("GetMyVendor");

        group.MapGet("/products", async (
            int? page,
            int? pageSize,
            ListMyProductsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new ListMyProductsQuery(page ?? 1, pageSize ?? 20),
                cancellationToken);

            return result.ToHttpResult(value => Results.Ok(value));
        })
        .WithName("ListMyProducts");

        group.MapPost("/products", async (
            CreateProductDraftRequest request,
            CreateProductDraftHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductDraftCommand(
                request.CategoryId,
                request.Name,
                request.Description,
                request.Variants
                    .Select(variant => new CreateProductVariantInput(
                        variant.Name,
                        variant.Sku,
                        variant.PriceKyat,
                        variant.WeightGrams,
                        variant.StockOnHand))
                    .ToList());

            var result = await handler.Handle(command, cancellationToken);

            return result.ToHttpResult(value =>
                Results.Created($"/api/v1/vendor/products/{value.Id}", value));
        })
        .WithName("CreateProductDraft");

        group.MapPost("/products/{productId:guid}/submit", async (
            Guid productId,
            SubmitProductForReviewHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new SubmitProductForReviewCommand(productId),
                cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("SubmitProductForReview");

        return app;
    }
}
