using System;
using System.Text.Json.Serialization;

namespace InventoryKpiSystem.Models
{
    public class PurchaseOrder
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitCost")]
        public decimal UnitCost { get; set; }

        [JsonPropertyName("purchaseDate")]
        public DateTime PurchaseDate { get; set; }
    }
}