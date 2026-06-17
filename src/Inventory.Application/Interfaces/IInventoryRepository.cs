using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task ReplaceAsync(
        IEnumerable<InventoryItem> inventoryItems,
        IEnumerable<StockMovement> stockMovements,
        CancellationToken cancellationToken = default);
}
