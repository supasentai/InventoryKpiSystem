using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;

namespace Inventory.Application.Services;

public class ImportService : IImportService
{
    private readonly IInventoryService _inventoryService;

    public ImportService(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public void ImportProducts(IEnumerable<Product> products)
    {
        foreach (var product in products)
        {
            _inventoryService.AddOrUpdateProduct(product);
        }
    }

    public void ImportInvoices(IEnumerable<Invoice> invoices)
    {
        foreach (var invoice in invoices)
        {
            foreach (var line in invoice.LineItems)
            {
                if (string.IsNullOrWhiteSpace(line.ItemCode))
                {
                    continue;
                }

                var quantity = (int)line.Quantity;

                if (invoice.Type == InvoiceType.AccountsPayable)
                {
                    _inventoryService.AddPurchase(line.ItemCode, quantity, line.UnitAmount, invoice.Date);
                }
                else if (invoice.Type == InvoiceType.AccountsReceivable)
                {
                    _inventoryService.AddSale(line.ItemCode, quantity, invoice.Date);
                }
            }
        }
    }
}
