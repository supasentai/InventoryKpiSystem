using System;
using System.Collections.Generic;

namespace InventoryKpiSystem.Models
{
    public class ProductInventory
    {
        public string ProductId { get; set; }
        
        public int PurchasedQuantity { get; set; }
        
        public int SoldQuantity { get; set; }
        
        public decimal UnitCost { get; set; }
        
        // Lưu trữ các ngày mua hàng để tính KPI 5 (Average Inventory Age)
        public List<DateTime> PurchaseDates { get; set; } = new List<DateTime>();

        // Thuộc tính tính toán phụ trợ (Tuỳ chọn)
        public int CurrentStock => PurchasedQuantity - SoldQuantity;
    }
}