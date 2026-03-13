using System;
using System.Collections.Generic;

namespace InventoryKpiSystem.Models
{
    public class Invoice
    {
        public string InvoiceID { get; set; } = "";
        public string InvoiceNumber { get; set; } = "";

        // ACCREC = sales invoice
        // ACCPAY = purchase invoice
        public string Type { get; set; } = "";

        public string ContactName { get; set; } = "";

        public DateTime Date { get; set; }

        public List<InvoiceLine> LineItems { get; set; } = new List<InvoiceLine>();

        // These properties are used by FileProcessor
        public string ProductId { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime InvoiceDate { get; set; }
    }

    public class InvoiceLine
    {
        public string ItemCode { get; set; } = "";

        public int Quantity { get; set; }

        public decimal UnitAmount { get; set; }

        public decimal LineAmount { get; set; }
    }
}