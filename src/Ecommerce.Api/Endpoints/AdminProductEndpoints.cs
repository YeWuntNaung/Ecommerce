using Ecommerce.Api.Common;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Catalog.Products.Review;
using Ecommerce.Contracts.Catalog;
using Ecommerce.Contracts.Common;

namespace Ecommerce.Api.Endpoints;

public static class AdminProductEndpoints
{
    public static IEndpointRouteBuilder MapAdminProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin")
            .WithTags("Admin")
            .RequireAuthorization("Admin");

        group.MapGet("/products/pending", async (
            int? page,
            int? pageSize,
            ICatalogQueries queries,
            CancellationToken cancellationToken) =>
        {
            var paging = new PagedRequest(page ?? 1, pageSize ?? 20);
            return Results.Ok(await queries.ListPendingProductsAsync(paging, cancellationToken));
        })
        .WithName("ListPendingProducts");

        group.MapPost("/products/{productId:guid}/approve", async (
            Guid productId,
            ApproveProductHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ApproveProductCommand(productId), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ApproveProduct");

        group.MapPost("/products/{productId:guid}/reject", async (
            Guid productId,
            RejectProductRequest request,
            RejectProductHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new RejectProductCommand(productId, request.Reason),
                cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("RejectProduct");

        return app;
    }
}
