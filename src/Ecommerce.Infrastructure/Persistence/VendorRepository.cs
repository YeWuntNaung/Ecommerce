using Ecommerce.Application.Abstractions;
using Ecommerce.Domain.Vendors;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence;

public sealed class VendorRepository(AppDbContext db) : IVendorRepository
{
    public Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Vendors.FirstOrDefaultAsync(vendor => vendor.Id == id, cancellationToken);

    public Task<Vendor?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var value = VendorSlug.FromTrusted(slug);
        return db.Vendors.FirstOrDefaultAsync(vendor => vendor.Slug == value, cancellationToken);
    }

    public Task<Vendor?> GetByOwnerUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Vendors.FirstOrDefaultAsync(vendor => vendor.OwnerUserId == userId, cancellationToken);
}
