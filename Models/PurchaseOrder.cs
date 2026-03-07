using System;
using System.Text.Json.Serialization;

namespace InventoryKpiSystem.Models
{
    public class PurchaseOrder
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("quantity_purchased")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_cost")]
        public decimal UnitCost { get; set; }

        [JsonPropertyName("purchase_date")]
        public DateTime PurchaseDate { get; set; }
    }
}