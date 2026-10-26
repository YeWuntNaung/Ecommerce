using Ecommerce.Contracts.Catalog;
using Ecommerce.Contracts.Common;

namespace Ecommerce.Application.Abstractions;

/// <summary>Identity of a storefront, carried into read queries so projections don't need extra joins.</summary>
public sealed record StorefrontRef(Guid Id, string Name, string Slug);

/// <summary>Read-model queries. Deliberately separate from repositories so list screens never load aggregates.</summary>
public interface ICatalogQueries
{
    Task<PagedResponse<ProductSummaryResponse>> ListStorefrontProductsAsync(
        StorefrontRef storefront,
        PagedRequest paging,
        CancellationToken cancellationToken);

    Task<StorefrontProductDetailResponse?> GetStorefrontProductAsync(
        StorefrontRef storefront,
        string productSlug,
        CancellationToken cancellationToken);

    Task<PagedResponse<VendorProductResponse>> ListVendorProductsAsync(
        Guid vendorId,
        PagedRequest paging,
        CancellationToken cancellationToken);

    Task<PagedResponse<PendingProductResponse>> ListPendingProductsAsync(
        PagedRequest paging,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryResponse>> ListCategoriesAsync(CancellationToken cancellationToken);

    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);
}
