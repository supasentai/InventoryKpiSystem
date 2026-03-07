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
        
        public List<DateTime> PurchaseDates { get; set; } = new List<DateTime>();
    }
}
