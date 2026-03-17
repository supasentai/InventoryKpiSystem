using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InventoryKpiSystem.Models
{
    public class Invoice
    {
        public string InvoiceID { get; set; } = "";

        public string InvoiceNumber { get; set; } = "";

        public string Type { get; set; } = ""; // Chứa ACCPAY hoặc ACCREC

        // Map DateString vào biến Date, ĐỒNG THỜI phớt lờ thuộc tính Date nguyên gốc của Xero
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

    public class InvoiceResponse
    {
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}