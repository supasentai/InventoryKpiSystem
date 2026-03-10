using System;
using System.Text.Json.Serialization;

namespace InventoryKpiSystem.Models
{
    public class Invoice
    {
        [JsonPropertyName("invoiceId")]
        public string InvoiceId { get; set; } = string.Empty;

        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("invoiceDate")]
        public DateTime InvoiceDate { get; set; }
    }
}