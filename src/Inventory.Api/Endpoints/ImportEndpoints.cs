using Inventory.Api.Extensions;
using Inventory.Api.Responses;
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
            HttpRequest request,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Inventory.Api.Endpoints.ImportEndpoints");

            if (request.ContentLength.GetValueOrDefault() > 0)
            {
                logger.LogWarning("Import request rejected because payloads are not supported.");

                return Results.Problem(
                    title: "Invalid request payload.",
                    detail: "POST /api/import/run does not accept a request body.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!Directory.Exists(paths.ProductsFolder) || !Directory.Exists(paths.InvoicesFolder))
            {
                logger.LogError(
                    "Configured import folders are invalid. ProductsFolder: {ProductsFolder}; InvoicesFolder: {InvoicesFolder}",
                    paths.ProductsFolder,
                    paths.InvoicesFolder);

                return Results.Problem(
                    title: "Import folders were not found.",
                    detail: "Product or invoice data folder was not found. Check the configured sample data paths.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            var productFiles = Directory.GetFiles(paths.ProductsFolder, "*.txt")
                .OrderBy(file => file)
                .ToList();

            var invoiceFiles = Directory.GetFiles(paths.InvoicesFolder, "*.txt")
                .OrderBy(file => file)
                .ToList();

            if (productFiles.Count == 0 || invoiceFiles.Count == 0)
            {
                logger.LogWarning(
                    "Import request found missing input files. ProductFiles: {ProductFileCount}; InvoiceFiles: {InvoiceFileCount}",
                    productFiles.Count,
                    invoiceFiles.Count);

                return Results.Problem(
                    title: "Import files were not found.",
                    detail: "At least one product file and one invoice file are required before running import.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            var products = new List<Product>();
            var invoices = new List<Invoice>();

            try
            {
                logger.LogInformation(
                    "Import execution started. ProductFileCount: {ProductFileCount}; InvoiceFileCount: {InvoiceFileCount}",
                    productFiles.Count,
                    invoiceFiles.Count);

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

                logger.LogInformation(
                    "Import execution completed. ProductsPersisted: {ProductsPersisted}; InvoicesPersisted: {InvoicesPersisted}; StockMovementsPersisted: {StockMovementsPersisted}; InventoryItems: {InventoryItems}",
                    products.Count,
                    invoices.Count,
                    stockMovements.Count,
                    inventoryService.Items.Count);

                return Results.Ok(ApiResponse<object>.Ok(new
                {
                    message = "Import completed.",
                    productFilesProcessed = productFiles.Count,
                    invoiceFilesProcessed = invoiceFiles.Count,
                    productsPersisted = products.Count,
                    invoicesPersisted = invoices.Count,
                    stockMovementsPersisted = stockMovements.Count,
                    inventoryItems = inventoryService.Items.Count,
                    persistedToDatabase = true
                }));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Import workflow failed.");

                return Results.Problem(
                    title: "Import failed.",
                    detail: "The import workflow failed while reading files or persisting data.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("RunImport")
        .WithTags("Import")
        .WithSummary("Runs the file-based product and invoice import workflow.")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

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
