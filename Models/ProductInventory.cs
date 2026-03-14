using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // BẮT BUỘC THÊM DÒNG NÀY

namespace InventoryKpiSystem.Models
{
    public class ProductInventory
    {
        // Chỉ đường: Lấy dữ liệu từ trường "ItemID" trong JSON bỏ vào ProductId
        [JsonPropertyName("ItemID")]
        public string ProductId { get; set; } = "";

        // Chỉ đường: Lấy dữ liệu từ trường "Code" trong JSON bỏ vào ItemCode
        [JsonPropertyName("Code")]
        public string ItemCode { get; set; } = "";

        public string Name { get; set; } = "";

        public int PurchasedQuantity { get; set; }

        public int SoldQuantity { get; set; }

        public int QuantityOnHand { get; set; }

        public decimal UnitCost { get; set; }

        public List<DateTime> PurchaseDates { get; set; } = new();
        public List<DateTime> SaleDates { get; set; } = new();
    }

    public class ProductResponse
    {
        public List<ProductInventory> Items { get; set; } = new List<ProductInventory>();
    }
}