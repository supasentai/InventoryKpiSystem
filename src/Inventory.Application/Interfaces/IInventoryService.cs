using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IInventoryService
{
    void AddOrUpdateProduct(Product product);

    void AddPurchase(string itemCode, int quantity, decimal unitCost, DateTime date);

    void AddSale(string itemCode, int quantity, DateTime date);

    List<InventoryItem> GetAllInventory();

    bool TryGetItem(string itemCode, out InventoryItem? item);

    IReadOnlyDictionary<string, InventoryItem> Items { get; }
}
