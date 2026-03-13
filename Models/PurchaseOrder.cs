using System;

namespace InventoryKpiSystem.Models
{
    public class PurchaseOrder
    {
        public string ProductId { get; set; } = "";

        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public DateTime PurchaseDate { get; set; }
    }

    public class PurchaseItem
    {
        public string ProductId { get; set; } = "";

        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public DateTime InvoiceDate { get; set; }
    }
}