namespace Inventory.Domain.ValueObjects;

public class KpiSnapshot
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    public int TotalSkus { get; set; }

    public int OutOfStockItems { get; set; }

    public double AverageDailySales { get; set; }

    public double AverageInventoryAge { get; set; }

    public decimal InventoryValue { get; set; }
}
