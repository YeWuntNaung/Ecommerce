using Ecommerce.Domain.Catalog;

namespace Ecommerce.Application.Abstractions;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(Guid vendorId, string slug, CancellationToken cancellationToken);
}
