using Ecommerce.Application.Abstractions;
using Ecommerce.Contracts.Catalog;
using Ecommerce.Contracts.Common;
using Ecommerce.Contracts.Vendors;
using Ecommerce.Domain.Primitives;

namespace Ecommerce.Application.Catalog.Storefront;

public sealed record GetStorefrontQuery(string Slug);

public sealed record ListStorefrontProductsQuery(string Slug, int Page = 1, int PageSize = 20);

public sealed record GetStorefrontProductQuery(string VendorSlug, string ProductSlug);

public sealed class GetStorefrontHandler(IVendorRepository vendors)
{
    public async Task<Result<StorefrontResponse>> Handle(GetStorefrontQuery query, CancellationToken cancellationToken)
    {
        var slug = SlugGenerator.From(query.Slug);
        var vendor = await vendors.GetBySlugAsync(slug, cancellationToken);

        if (vendor is null || !vendor.CanSell)
        {
            return Result.Fail<StorefrontResponse>(Error.NotFound("Storefront not found."));
        }

        return Result.Ok(new StorefrontResponse(vendor.Id, vendor.Name, vendor.Slug.Value, vendor.Description));
    }
}

public sealed class ListStorefrontProductsHandler(IVendorRepository vendors, ICatalogQueries catalogQueries)
{
    public async Task<Result<PagedResponse<ProductSummaryResponse>>> Handle(
        ListStorefrontProductsQuery query,
        CancellationToken cancellationToken)
    {
        var slug = SlugGenerator.From(query.Slug);
        var vendor = await vendors.GetBySlugAsync(slug, cancellationToken);

        if (vendor is null || !vendor.CanSell)
        {
            return Result.Fail<PagedResponse<ProductSummaryResponse>>(Error.NotFound("Storefront not found."));
        }

        var page = new PagedRequest(query.Page, query.PageSize);
        var storefront = new StorefrontRef(vendor.Id, vendor.Name, vendor.Slug.Value);
        var products = await catalogQueries.ListStorefrontProductsAsync(storefront, page, cancellationToken);
        return Result.Ok(products);
    }
}

public sealed class GetStorefrontProductHandler(IVendorRepository vendors, ICatalogQueries catalogQueries)
{
    public async Task<Result<StorefrontProductDetailResponse>> Handle(
        GetStorefrontProductQuery query,
        CancellationToken cancellationToken)
    {
        var vendor = await vendors.GetBySlugAsync(SlugGenerator.From(query.VendorSlug), cancellationToken);

        if (vendor is null || !vendor.CanSell)
        {
            return Result.Fail<StorefrontProductDetailResponse>(Error.NotFound("Storefront not found."));
        }

        var product = await catalogQueries.GetStorefrontProductAsync(
            new StorefrontRef(vendor.Id, vendor.Name, vendor.Slug.Value),
            SlugGenerator.From(query.ProductSlug),
            cancellationToken);

        return product is null
            ? Result.Fail<StorefrontProductDetailResponse>(Error.NotFound("Product not found."))
            : Result.Ok(product);
    }
}
