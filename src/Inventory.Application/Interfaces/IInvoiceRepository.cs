using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task ReplaceAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken = default);
}
