using System;
using System.Text.Json.Serialization;

namespace InventoryKpiSystem.Models
{
    public class Invoice
    {
        [JsonPropertyName("invoice_id")]
        public string InvoiceId { get; set; }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("quantity_sold")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_selling_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("invoice_date")]
        public DateTime InvoiceDate { get; set; }
    }
}