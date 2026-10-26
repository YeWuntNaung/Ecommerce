namespace Ecommerce.Contracts.Catalog;

public sealed record ProductSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    long PriceFromKyat,
    string Currency,
    string VendorName,
    string VendorSlug,
    string CategoryName);

public sealed record ProductVariantResponse(
    Guid Id,
    string Name,
    string Sku,
    long PriceKyat,
    string Currency,
    int WeightGrams,
    int StockOnHand);

public sealed record StorefrontProductDetailResponse(
    Guid Id,
    Guid VendorId,
    string Name,
    string Slug,
    string Description,
    string CategoryName,
    string VendorName,
    string VendorSlug,
    IReadOnlyList<ProductVariantResponse> Variants);

public sealed record VendorProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    int VariantCount,
    DateTimeOffset UpdatedAt,
    string? RejectionReason);

public sealed record PendingProductResponse(
    Guid Id,
    string Name,
    string VendorName,
    string CategoryName,
    int VariantCount,
    DateTimeOffset SubmittedAt);

public sealed record CategoryResponse(Guid Id, string Name, string Slug);

public sealed record CreateProductVariantRequest(
    string Name,
    string Sku,
    long PriceKyat,
    int WeightGrams,
    int StockOnHand);

public sealed record CreateProductDraftRequest(
    Guid CategoryId,
    string Name,
    string Description,
    IReadOnlyList<CreateProductVariantRequest> Variants);

public sealed record CreateProductDraftResponse(Guid Id, string Slug);

public sealed record RejectProductRequest(string Reason);
