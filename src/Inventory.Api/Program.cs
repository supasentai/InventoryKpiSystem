using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Inventory.Infrastructure.FileParsing;
using Inventory.Infrastructure.Json;
using Inventory.Infrastructure.Reporting;

var builder = WebApplication.CreateBuilder(args);

var dataPaths = ResolveDataPaths(builder.Environment.ContentRootPath);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(dataPaths);
builder.Services.AddSingleton<IFifoCostingService, FifoCostingService>();
builder.Services.AddSingleton<IInventorySnapshotStore>(_ =>
    new JsonInventorySnapshotStore(Path.Combine(dataPaths.RuntimeRoot, "inventory-snapshot.json")));
builder.Services.AddSingleton<IInventoryService>(provider =>
{
    var fifoCostingService = provider.GetRequiredService<IFifoCostingService>();
    var snapshotStore = provider.GetRequiredService<IInventorySnapshotStore>();
    return new InventoryService(fifoCostingService, snapshotStore.Load());
});
builder.Services.AddSingleton<IImportService, ImportService>();
builder.Services.AddSingleton<IKpiService, KpiService>();
builder.Services.AddSingleton<IProductFileReader, ProductFileReader>();
builder.Services.AddSingleton<IInvoiceFileReader, InvoiceFileReader>();
builder.Services.AddSingleton<IProcessedFileRegistry>(_ =>
    new ProcessedFileRegistry(Path.Combine(dataPaths.RuntimeRoot, "processed-files")));
builder.Services.AddSingleton<IReportWriter>(_ =>
    new JsonReportWriter(Path.Combine(dataPaths.RuntimeRoot, "reports")));
builder.Services.AddSingleton<FileProcessor>();

var app = builder.Build();

app.MapOpenApi();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    checkedAt = DateTime.UtcNow
}))
.WithName("Health");

app.MapGet("/api/products", (IInventoryService inventoryService) =>
{
    var products = inventoryService.GetAllInventory()
        .OrderBy(item => item.ItemCode)
        .Select(item => new
        {
            item.ProductId,
            item.ItemCode,
            item.Name
        });

    return Results.Ok(products);
})
.WithName("GetProducts");

app.MapGet("/api/inventory", (IInventoryService inventoryService) =>
{
    var inventory = inventoryService.GetAllInventory()
        .OrderBy(item => item.ItemCode)
        .Select(item => new
        {
            item.ProductId,
            item.ItemCode,
            item.Name,
            item.QuantityOnHand,
            item.TotalSoldQuantity,
            item.TotalStockValue,
            PurchaseBatches = item.PurchaseBatches
                .OrderBy(batch => batch.PurchaseDate)
                .Select(batch => new
                {
                    batch.PurchaseDate,
                    batch.UnitCost,
                    batch.InitialQuantity,
                    batch.RemainingQuantity
                })
        });

    return Results.Ok(inventory);
})
.WithName("GetInventory");

app.MapGet("/api/kpis", (IInventoryService inventoryService, IKpiService kpiService) =>
{
    var snapshot = kpiService.CreateSnapshot(inventoryService.GetAllInventory());
    return Results.Ok(snapshot);
})
.WithName("GetKpis");

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
.WithName("RunImport");

app.Run();

static ApiDataPaths ResolveDataPaths(string contentRoot)
{
    var currentDirectory = Directory.GetCurrentDirectory();
    var candidates = new[]
    {
        Path.GetFullPath(Path.Combine(currentDirectory, "InventoryKpiSystem")),
        Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "InventoryKpiSystem")),
        Path.GetFullPath(Path.Combine(contentRoot, "InventoryKpiSystem"))
    };

    var runtimeRoot = candidates.FirstOrDefault(path =>
        Directory.Exists(Path.Combine(path, "Data", "Products")) &&
        Directory.Exists(Path.Combine(path, "Data", "Invoices"))) ?? candidates[0];

    return new ApiDataPaths(
        Path.Combine(runtimeRoot, "Data", "Products"),
        Path.Combine(runtimeRoot, "Data", "Invoices"),
        runtimeRoot);
}

internal sealed record ApiDataPaths(
    string ProductsFolder,
    string InvoicesFolder,
    string RuntimeRoot);
