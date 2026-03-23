using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace InventoryKpiSystem.Models
{
    // Cấu trúc Lô hàng FIFO
    public class PurchaseBatch
    {
        public DateTime PurchaseDate { get; set; }
        public decimal UnitCost { get; set; }
        public int InitialQuantity { get; set; }
        public int RemainingQuantity { get; set; }

        public double GetAgeInDays(DateTime currentDate)
        {
            return (currentDate - PurchaseDate).TotalDays;
        }
    }

    // Cấu trúc Sản phẩm
    public class ProductInventory
    {
        [JsonPropertyName("ItemID")]
        public string ProductId { get; set; } = "";

        [JsonPropertyName("Code")]
        public string ItemCode { get; set; } = "";

        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        public List<PurchaseBatch> PurchaseBatches { get; set; } = new();

        public int TotalSoldQuantity { get; set; }

        public List<DateTime> SaleDates { get; set; } = new();

        public int QuantityOnHand => PurchaseBatches.Sum(b => b.RemainingQuantity);

        public decimal TotalStockValue => PurchaseBatches.Sum(b => b.RemainingQuantity * b.UnitCost);
    }

    public class ProductResponse
    {
        public List<ProductInventory> Items { get; set; } = new List<ProductInventory>();
    }
}