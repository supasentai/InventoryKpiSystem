using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InventoryKpiSystem.Models
{
    public class ProductInventory
    {
        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("purchasedQuantity")]
        public int PurchasedQuantity { get; set; }

        [JsonPropertyName("soldQuantity")]
        public int SoldQuantity { get; set; }

        [JsonPropertyName("unitCost")]
        public decimal UnitCost { get; set; }

        [JsonPropertyName("purchaseDates")]
        public List<DateTime> PurchaseDates { get; set; } = new List<DateTime>();
    }
}