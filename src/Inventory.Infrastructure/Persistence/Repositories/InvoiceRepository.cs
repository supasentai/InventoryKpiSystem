using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InventoryDbContext _dbContext;

    public InvoiceRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.LineItems)
            .OrderBy(invoice => invoice.Date)
            .ThenBy(invoice => invoice.InvoiceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken = default)
    {
        await _dbContext.InvoiceLines.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Invoices.ExecuteDeleteAsync(cancellationToken);

        var uniqueInvoices = invoices
            .Where(invoice => !string.IsNullOrWhiteSpace(invoice.InvoiceID))
            .GroupBy(invoice => invoice.InvoiceID)
            .Select(group => NormalizeInvoice(group.Last()))
            .ToList();

        await _dbContext.Invoices.AddRangeAsync(uniqueInvoices, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Invoice NormalizeInvoice(Invoice invoice)
    {
        return new Invoice
        {
            InvoiceID = invoice.InvoiceID,
            InvoiceNumber = invoice.InvoiceNumber,
            Type = invoice.Type,
            Date = AsUtc(invoice.Date),
            LineItems = invoice.LineItems
                .Where(line => !string.IsNullOrWhiteSpace(line.ItemCode))
                .Select(line => new InvoiceLine
                {
                    ItemCode = line.ItemCode,
                    Quantity = line.Quantity,
                    UnitAmount = line.UnitAmount
                })
                .ToList()
        };
    }

    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
