namespace Inventory.Domain.Entities;

public class InventoryItem
{
    public string ProductId { get; set; } = string.Empty;

    public string ItemCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<StockLot> PurchaseBatches { get; set; } = new();

    public int TotalSoldQuantity { get; set; }

    public List<DateTime> SaleDates { get; set; } = new();

    public int QuantityOnHand => PurchaseBatches.Sum(batch => batch.RemainingQuantity);

    public decimal TotalStockValue => PurchaseBatches.Sum(batch => batch.RemainingQuantity * batch.UnitCost);
}
