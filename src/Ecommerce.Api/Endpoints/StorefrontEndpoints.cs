using Ecommerce.Api.Common;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Catalog.Storefront;

namespace Ecommerce.Api.Endpoints;

public static class StorefrontEndpoints
{
    public static IEndpointRouteBuilder MapStorefrontEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Storefront")
            .AllowAnonymous();

        group.MapGet("/categories", async (ICatalogQueries queries, CancellationToken cancellationToken) =>
            Results.Ok(await queries.ListCategoriesAsync(cancellationToken)))
            .WithName("ListCategories");

        group.MapGet("/stores/{slug}", async (
            string slug,
            GetStorefrontHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new GetStorefrontQuery(slug), cancellationToken);
            return result.ToHttpResult(value => Results.Ok(value));
        })
        .WithName("GetStorefront");

        group.MapGet("/stores/{slug}/products", async (
            string slug,
            int? page,
            int? pageSize,
            ListStorefrontProductsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new ListStorefrontProductsQuery(slug, page ?? 1, pageSize ?? 20),
                cancellationToken);

            return result.ToHttpResult(value => Results.Ok(value));
        })
        .WithName("ListStorefrontProducts");

        group.MapGet("/stores/{slug}/products/{productSlug}", async (
            string slug,
            string productSlug,
            GetStorefrontProductHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new GetStorefrontProductQuery(slug, productSlug),
                cancellationToken);

            return result.ToHttpResult(value => Results.Ok(value));
        })
        .WithName("GetStorefrontProduct");

        return app;
    }
}
