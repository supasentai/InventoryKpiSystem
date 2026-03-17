using System;

namespace InventoryKpiSystem.DTOs
{
    public class ProductKpiResult
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TotalPurchased { get; set; }
        public int TotalSold { get; set; }
        public int QuantityOnHand { get; set; }
        public decimal CurrentStockValue { get; set; }
    }
}