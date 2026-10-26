using Ecommerce.Application.Abstractions;
using Ecommerce.Contracts.Catalog;
using Ecommerce.Contracts.Common;
using Ecommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence;

public sealed class CatalogQueries(AppDbContext db) : ICatalogQueries
{
    public async Task<PagedResponse<ProductSummaryResponse>> ListStorefrontProductsAsync(
        StorefrontRef storefront,
        PagedRequest paging,
        CancellationToken cancellationToken)
    {
        var page = paging.NormalizedPage;
        var size = paging.NormalizedPageSize;

        var query = db.Products
            .AsNoTracking()
            .Where(product => product.VendorId == storefront.Id && product.Status == ProductStatus.Approved);

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(product => new
            {
                product.Id,
                product.Name,
                product.Slug,
                PriceFromKyat = product.Variants.Min(variant => (long?)variant.PriceKyat) ?? 0,
                CategoryName = db.Categories
                    .Where(category => category.Id == product.CategoryId)
                    .Select(category => category.Name)
                    .FirstOrDefault() ?? "Uncategorised",
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new ProductSummaryResponse(
                row.Id,
                row.Name,
                row.Slug,
                row.PriceFromKyat,
                CurrencyCodes.Mmk,
                storefront.Name,
                storefront.Slug,
                row.CategoryName))
            .ToList();

        return new PagedResponse<ProductSummaryResponse>(items, page, size, total);
    }

    public async Task<StorefrontProductDetailResponse?> GetStorefrontProductAsync(
        StorefrontRef storefront,
        string productSlug,
        CancellationToken cancellationToken)
    {
        var row = await db.Products
            .AsNoTracking()
            .Where(product => product.VendorId == storefront.Id
                              && product.Slug == productSlug
                              && product.Status == ProductStatus.Approved)
            .Select(product => new
            {
                product.Id,
                product.Name,
                product.Slug,
                product.Description,
                CategoryName = db.Categories
                    .Where(category => category.Id == product.CategoryId)
                    .Select(category => category.Name)
                    .FirstOrDefault() ?? "Uncategorised",
                Variants = product.Variants
                    .OrderBy(variant => variant.PriceKyat)
                    .Select(variant => new
                    {
                        variant.Id,
                        variant.Name,
                        variant.Sku,
                        variant.PriceKyat,
                        variant.WeightGrams,
                        variant.StockOnHand,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new StorefrontProductDetailResponse(
            row.Id,
            storefront.Id,
            row.Name,
            row.Slug,
            row.Description,
            row.CategoryName,
            storefront.Name,
            storefront.Slug,
            row.Variants
                .Select(variant => new ProductVariantResponse(
                    variant.Id,
                    variant.Name,
                    variant.Sku,
                    variant.PriceKyat,
                    CurrencyCodes.Mmk,
                    variant.WeightGrams,
                    variant.StockOnHand))
                .ToList());
    }

    public async Task<PagedResponse<VendorProductResponse>> ListVendorProductsAsync(
        Guid vendorId,
        PagedRequest paging,
        CancellationToken cancellationToken)
    {
        var page = paging.NormalizedPage;
        var size = paging.NormalizedPageSize;

        var query = db.Products
            .AsNoTracking()
            .Where(product => product.VendorId == vendorId);

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(product => product.UpdatedAt)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(product => new
            {
                product.Id,
                product.Name,
                product.Slug,
                product.Status,
                VariantCount = product.Variants.Count,
                product.UpdatedAt,
                product.RejectionReason,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new VendorProductResponse(
                row.Id,
                row.Name,
                row.Slug,
                row.Status.ToString(),
                row.VariantCount,
                row.UpdatedAt,
                row.RejectionReason))
            .ToList();

        return new PagedResponse<VendorProductResponse>(items, page, size, total);
    }

    public async Task<PagedResponse<PendingProductResponse>> ListPendingProductsAsync(
        PagedRequest paging,
        CancellationToken cancellationToken)
    {
        var page = paging.NormalizedPage;
        var size = paging.NormalizedPageSize;

        var query = from product in db.Products.AsNoTracking()
                    where product.Status == ProductStatus.PendingReview
                    join vendor in db.Vendors on product.VendorId equals vendor.Id
                    join category in db.Categories on product.CategoryId equals category.Id
                    select new
                    {
                        product.Id,
                        product.Name,
                        VendorName = vendor.Name,
                        CategoryName = category.Name,
                        VariantCount = product.Variants.Count,
                        SubmittedAt = product.SubmittedAt ?? product.CreatedAt,
                    };

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(row => row.SubmittedAt)
            .ThenBy(row => row.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(row => new PendingProductResponse(
                row.Id,
                row.Name,
                row.VendorName,
                row.CategoryName,
                row.VariantCount,
                row.SubmittedAt))
            .ToListAsync(cancellationToken);

        return new PagedResponse<PendingProductResponse>(items, page, size, total);
    }

    public async Task<IReadOnlyList<CategoryResponse>> ListCategoriesAsync(CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new CategoryResponse(category.Id, category.Name, category.Slug))
            .ToListAsync(cancellationToken);

    public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(category => category.Id == categoryId && category.IsActive, cancellationToken);
}
