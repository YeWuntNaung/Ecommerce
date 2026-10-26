namespace Ecommerce.Contracts.Vendors;

public sealed record StorefrontResponse(Guid Id, string Name, string Slug, string Description);

public sealed record VendorProfileResponse(Guid Id, string Name, string Slug, string Status, string Description);
