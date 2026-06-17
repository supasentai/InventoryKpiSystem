using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task ReplaceAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);
}
