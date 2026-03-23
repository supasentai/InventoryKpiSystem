using System.Collections.Generic;
using System.Threading;
using InventoryKpiSystem.Models; 

namespace InventoryKpiSystem.Loaders
{
    public interface IJsonFileLoader
    {
        IAsyncEnumerable<PurchaseOrder> LoadPurchaseOrdersAsync(string filePath, CancellationToken cancellationToken = default);

        IAsyncEnumerable<Invoice> LoadInvoicesAsync(string filePath, CancellationToken cancellationToken = default);
    }
}