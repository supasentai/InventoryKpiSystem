using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IImportService
{
    void ImportProducts(IEnumerable<Product> products);

    void ImportInvoices(IEnumerable<Invoice> invoices);
}
