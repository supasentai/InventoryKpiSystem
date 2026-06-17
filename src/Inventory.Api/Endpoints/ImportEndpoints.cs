using Inventory.Api.Extensions;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.FileParsing;

namespace Inventory.Api.Endpoints;

internal static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/import/run", async (
            ApiDataPaths paths,
            FileProcessor processor,
            IProductFileReader productFileReader,
            IInvoiceFileReader invoiceFileReader,
            IInventoryService inventoryService,
            IInventorySnapshotStore snapshotStore,
            IProductRepository productRepository,
            IInvoiceRepository invoiceRepository,
            IInventoryRepository inventoryRepository,
            CancellationToken cancellationToken) =>
        {
            if (!Directory.Exists(paths.ProductsFolder) || !Directory.Exists(paths.InvoicesFolder))
            {
                return Results.NotFound(new
                {
                    message = "Product or invoice data folder was not found.",
                    paths.ProductsFolder,
                    paths.InvoicesFolder
                });
            }

            var productFiles = Directory.GetFiles(paths.ProductsFolder, "*.txt")
                .OrderBy(file => file)
                .ToList();

            var invoiceFiles = Directory.GetFiles(paths.InvoicesFolder, "*.txt")
                .OrderBy(file => file)
                .ToList();

            var products = new List<Product>();
            var invoices = new List<Invoice>();

            foreach (var file in productFiles)
            {
                products.AddRange(await productFileReader.ReadAsync(file, cancellationToken));
                await processor.ProcessProductFileAsync(file, cancellationToken);
            }

            foreach (var file in invoiceFiles)
            {
                invoices.AddRange(await invoiceFileReader.ReadAsync(file, cancellationToken));
                await processor.ProcessInvoiceFileAsync(file, cancellationToken);
            }

            var stockMovements = CreateStockMovements(invoices);

            snapshotStore.Save(inventoryService.Items);
            await productRepository.ReplaceAsync(products, cancellationToken);
            await invoiceRepository.ReplaceAsync(invoices, cancellationToken);
            await inventoryRepository.ReplaceAsync(
                inventoryService.GetAllInventory(),
                stockMovements,
                cancellationToken);

            return Results.Ok(new
            {
                message = "Import completed.",
                productFilesProcessed = productFiles.Count,
                invoiceFilesProcessed = invoiceFiles.Count,
                productsPersisted = products.Count,
                invoicesPersisted = invoices.Count,
                stockMovementsPersisted = stockMovements.Count,
                inventoryItems = inventoryService.Items.Count,
                persistedToDatabase = true
            });
        })
        .WithName("RunImport")
        .WithTags("Import")
        .WithSummary("Runs the file-based product and invoice import workflow.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static List<StockMovement> CreateStockMovements(IEnumerable<Invoice> invoices)
    {
        return invoices
            .SelectMany(invoice => invoice.LineItems
                .Where(line => !string.IsNullOrWhiteSpace(line.ItemCode))
                .Select(line => CreateStockMovement(invoice, line)))
            .Where(movement => movement is not null)
            .Select(movement => movement!)
            .ToList();
    }

    private static StockMovement? CreateStockMovement(Invoice invoice, InvoiceLine line)
    {
        var quantity = (int)line.Quantity;

        if (invoice.Type == InvoiceType.AccountsPayable)
        {
            return new StockMovement
            {
                ItemCode = line.ItemCode,
                Type = StockMovementType.Purchase,
                Quantity = quantity,
                UnitCost = line.UnitAmount,
                MovementDate = invoice.Date
            };
        }

        if (invoice.Type == InvoiceType.AccountsReceivable)
        {
            return new StockMovement
            {
                ItemCode = line.ItemCode,
                Type = StockMovementType.Sale,
                Quantity = quantity,
                UnitCost = line.UnitAmount,
                MovementDate = invoice.Date
            };
        }

        return null;
    }
}
