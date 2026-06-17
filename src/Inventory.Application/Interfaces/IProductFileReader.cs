using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IProductFileReader
{
    Task<IReadOnlyList<Product>> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
