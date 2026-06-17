namespace Inventory.Domain.Entities;

public class InvoiceLine
{
    public string ItemCode { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitAmount { get; set; }
}
