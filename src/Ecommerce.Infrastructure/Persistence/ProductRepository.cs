using Ecommerce.Application.Abstractions;
using Ecommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence;

public sealed class ProductRepository(AppDbContext db) : IProductRepository
{
    public Task AddAsync(Product product, CancellationToken cancellationToken) =>
        db.Products.AddAsync(product, cancellationToken).AsTask();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Products
            .Include(product => product.Variants)
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(Guid vendorId, string slug, CancellationToken cancellationToken) =>
        db.Products.AnyAsync(product => product.VendorId == vendorId && product.Slug == slug, cancellationToken);
}
