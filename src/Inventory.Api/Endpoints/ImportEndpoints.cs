using Inventory.Api.Extensions;
using Inventory.Application.Interfaces;
using Inventory.Infrastructure.FileParsing;

namespace Inventory.Api.Endpoints;

internal static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/import/run", async (
            ApiDataPaths paths,
            FileProcessor processor,
            IInventoryService inventoryService,
            IInventorySnapshotStore snapshotStore,
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

            foreach (var file in productFiles)
            {
                await processor.ProcessProductFileAsync(file, cancellationToken);
            }

            foreach (var file in invoiceFiles)
            {
                await processor.ProcessInvoiceFileAsync(file, cancellationToken);
            }

            snapshotStore.Save(inventoryService.Items);

            return Results.Ok(new
            {
                message = "Import completed.",
                productFilesProcessed = productFiles.Count,
                invoiceFilesProcessed = invoiceFiles.Count,
                inventoryItems = inventoryService.Items.Count
            });
        })
        .WithName("RunImport")
        .WithTags("Import")
        .WithSummary("Runs the file-based product and invoice import workflow.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
