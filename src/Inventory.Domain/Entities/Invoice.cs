using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class Invoice
{
    public string InvoiceID { get; set; } = string.Empty;

    public string InvoiceNumber { get; set; } = string.Empty;

    public InvoiceType Type { get; set; } = InvoiceType.Unknown;

    public DateTime Date { get; set; }

    public List<InvoiceLine> LineItems { get; set; } = new();
}
