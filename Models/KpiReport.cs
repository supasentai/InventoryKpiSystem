namespace InventoryKpiSystem.Models
{
    public class KpiReport
    {
        public int TotalSkus { get; set; }

        public decimal InventoryValue { get; set; }

        public int OutOfStockItems { get; set; }

        public double AverageDailySales { get; set; }

        public double AverageInventoryAge { get; set; }
    }
}