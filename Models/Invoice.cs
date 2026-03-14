using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Thư viện bắt buộc phải có để map JSON

namespace InventoryKpiSystem.Models
{
    public class Invoice
    {
        public string InvoiceID { get; set; } = "";
        
        public string InvoiceNumber { get; set; } = "";
        
        public string Type { get; set; } = "";
        
        public string ContactName { get; set; } = "";

        // BÍ QUYẾT LÀ ĐÂY: Bảo C# lấy dữ liệu sạch từ trường "DateString" của JSON
        [JsonPropertyName("DateString")]
        public DateTime Date { get; set; }

        public List<InvoiceLine> LineItems { get; set; } = new List<InvoiceLine>();
    }

    public class InvoiceLine
    {
        public string ItemCode { get; set; } = "";
        
        public decimal Quantity { get; set; }
        
        public decimal UnitAmount { get; set; }
    }

    // Class này làm nhiệm vụ bóc lớp vỏ { "Invoices": [ ... ] }
    public class InvoiceResponse
    {
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}