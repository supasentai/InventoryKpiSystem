using System;

namespace InventoryKpiSystem.Models
{
    public class KpiReport
    {
        public int TotalSkus { get; set; }

        public int OutOfStockItems { get; set; }

        public double AverageDailySales { get; set; }

        public double AverageInventoryAge { get; set; }

        public decimal TotalSales { get; set; }

        public decimal TotalPurchases { get; set; }

        public decimal InventoryValue { get; set; }

        public int TotalOrders { get; set; }

        public int TotalProducts { get; set; }

        public void PrintReport()
        {
            Console.WriteLine("===== KPI REPORT =====");

            Console.WriteLine($"Total SKUs: {TotalSkus}");
            Console.WriteLine($"Out of Stock Items: {OutOfStockItems}");
            Console.WriteLine($"Average Daily Sales: {AverageDailySales}");
            Console.WriteLine($"Average Inventory Age: {AverageInventoryAge}");

            Console.WriteLine($"Total Sales: {TotalSales}");
            Console.WriteLine($"Total Purchases: {TotalPurchases}");
            Console.WriteLine($"Inventory Value: {InventoryValue}");
            Console.WriteLine($"Total Orders: {TotalOrders}");
            Console.WriteLine($"Total Products: {TotalProducts}");
        }
    }
}