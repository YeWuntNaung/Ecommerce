using Ecommerce.Application.Abstractions;
using Ecommerce.Contracts.Catalog;
using Ecommerce.Contracts.Common;
using Ecommerce.Contracts.Vendors;
using Ecommerce.Domain.Primitives;

namespace Ecommerce.Application.Vendors;

public sealed record ListMyProductsQuery(int Page = 1, int PageSize = 20);

public sealed class ListMyProductsHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    ICatalogQueries catalogQueries)
{
    public async Task<Result<PagedResponse<VendorProductResponse>>> Handle(
        ListMyProductsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Fail<PagedResponse<VendorProductResponse>>(Error.Unauthorized());
        }

        var vendor = await vendors.GetByOwnerUserIdAsync(userId, cancellationToken);

        if (vendor is null)
        {
            return Result.Fail<PagedResponse<VendorProductResponse>>(
                Error.Forbidden("Your account is not linked to a vendor storefront."));
        }

        var products = await catalogQueries.ListVendorProductsAsync(
            vendor.Id,
            new PagedRequest(query.Page, query.PageSize),
            cancellationToken);

        return Result.Ok(products);
    }
}

public sealed class GetMyVendorHandler(ICurrentUser currentUser, IVendorRepository vendors)
{
    public async Task<Result<VendorProfileResponse>> Handle(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Fail<VendorProfileResponse>(Error.Unauthorized());
        }

        var vendor = await vendors.GetByOwnerUserIdAsync(userId, cancellationToken);

        if (vendor is null)
        {
            return Result.Fail<VendorProfileResponse>(
                Error.Forbidden("Your account is not linked to a vendor storefront."));
        }

        return Result.Ok(new VendorProfileResponse(
            vendor.Id,
            vendor.Name,
            vendor.Slug.Value,
            vendor.Status.ToString(),
            vendor.Description));
    }
}
