using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IInvoiceFileReader
{
    Task<IReadOnlyList<Invoice>> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
