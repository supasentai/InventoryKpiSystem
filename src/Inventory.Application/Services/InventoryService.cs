using System.Collections.Concurrent;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IFifoCostingService _fifoCostingService;
    private readonly ConcurrentDictionary<string, InventoryItem> _items;

    public InventoryService(IFifoCostingService fifoCostingService)
        : this(fifoCostingService, null)
    {
    }

    public InventoryService(
        IFifoCostingService fifoCostingService,
        IReadOnlyDictionary<string, InventoryItem>? initialItems)
    {
        _fifoCostingService = fifoCostingService;
        _items = initialItems is null
            ? new ConcurrentDictionary<string, InventoryItem>()
            : new ConcurrentDictionary<string, InventoryItem>(initialItems);
    }

    public IReadOnlyDictionary<string, InventoryItem> Items => _items;

    public void AddOrUpdateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.ItemCode))
        {
            return;
        }

        var existingProduct = _items.GetOrAdd(
            product.ItemCode,
            itemCode => new InventoryItem { ItemCode = itemCode });

        lock (existingProduct)
        {
            if (!string.IsNullOrWhiteSpace(product.Name))
            {
                existingProduct.Name = product.Name;
            }

            if (!string.IsNullOrWhiteSpace(product.ProductId))
            {
                existingProduct.ProductId = product.ProductId;
            }
        }
    }

    public void AddPurchase(string itemCode, int quantity, decimal unitCost, DateTime date)
    {
        var product = GetOrCreateItem(itemCode);

        lock (product)
        {
            product.PurchaseBatches.Add(new StockLot
            {
                PurchaseDate = date,
                UnitCost = unitCost,
                InitialQuantity = quantity,
                RemainingQuantity = quantity
            });
        }
    }

    public void AddSale(string itemCode, int quantity, DateTime date)
    {
        var product = GetOrCreateItem(itemCode);

        lock (product)
        {
            product.TotalSoldQuantity += quantity;
            product.SaleDates.Add(date);
            _fifoCostingService.ApplySale(product, quantity);
        }
    }

    public List<InventoryItem> GetAllInventory()
    {
        return _items.Values.ToList();
    }

    public bool TryGetItem(string itemCode, out InventoryItem? item)
    {
        return _items.TryGetValue(itemCode, out item);
    }

    private InventoryItem GetOrCreateItem(string itemCode)
    {
        return _items.GetOrAdd(
            itemCode,
            id => new InventoryItem
            {
                ItemCode = id,
                ProductId = Guid.NewGuid().ToString()
            });
    }
}
