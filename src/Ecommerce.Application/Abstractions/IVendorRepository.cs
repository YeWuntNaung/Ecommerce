using Ecommerce.Domain.Vendors;

namespace Ecommerce.Application.Abstractions;

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Vendor?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<Vendor?> GetByOwnerUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
