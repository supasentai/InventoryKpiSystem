using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InventoryKpiSystem.Models
{
    public class ProductInventory
    {
        [JsonPropertyName("ItemID")]
        public string ProductId { get; set; } = "";

        [JsonPropertyName("Code")]
        public string ItemCode { get; set; } = "";

        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        // Các trường này tự hệ thống ta tính toán, không lấy từ JSON
        [JsonIgnore]
        public int PurchasedQuantity { get; set; }

        [JsonIgnore]
        public int SoldQuantity { get; set; }

        [JsonIgnore]
        public decimal UnitCost { get; set; }

        [JsonIgnore]
        public List<DateTime> PurchaseDates { get; set; } = new();

        [JsonIgnore]
        public List<DateTime> SaleDates { get; set; } = new();
        public decimal QuantityOnHand => Math.Max(0, PurchasedQuantity - SoldQuantity);


    }

    public class ProductResponse
    {
        public List<ProductInventory> Items { get; set; } = new List<ProductInventory>();
    }
}