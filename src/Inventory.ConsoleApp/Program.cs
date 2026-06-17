using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Inventory.ConsoleApp.Presentation;
using Inventory.Infrastructure.FileParsing;
using Inventory.Infrastructure.Json;
using Inventory.Infrastructure.Reporting;

namespace Inventory.ConsoleApp;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("=== INVENTORY KPI SYSTEM STARTING ===");

        var invoicesFolder = ResolveDataFolder("Invoices");
        var productsFolder = ResolveDataFolder("Products");
        var runtimeRoot = ResolveRuntimeRoot(invoicesFolder);

        var snapshotStore = new JsonInventorySnapshotStore(Path.Combine(runtimeRoot, "inventory-snapshot.json"));
        var initialItems = snapshotStore.Load();

        IFifoCostingService fifoCostingService = new FifoCostingService();
        IInventoryService inventoryService = new InventoryService(fifoCostingService, initialItems);
        IImportService importService = new ImportService(inventoryService);
        IKpiService kpiService = new KpiService();
        IProcessedFileRegistry registry = new ProcessedFileRegistry(Path.Combine(runtimeRoot, "processed-files"));
        IProductFileReader productFileReader = new ProductFileReader();
        IInvoiceFileReader invoiceFileReader = new InvoiceFileReader();
        IReportWriter reportWriter = new JsonReportWriter(Path.Combine(runtimeRoot, "reports"));

        var processor = new FileProcessor(productFileReader, invoiceFileReader, importService, registry);
        var queue = new FileProcessingQueue(1000);
        Action onFileProcessed = () => Console.Write("\nChoose an option (0-3): ");

        var watcher = new FolderWatcher(
            invoicesFolder,
            productsFolder,
            processor,
            queue,
            onFileProcessed,
            inventoryService,
            snapshotStore);

        Console.WriteLine();
        Console.WriteLine("[System] Syncing historical data in file order...");

        await ImportHistoricalProducts(productsFolder, processor);
        await ImportHistoricalInvoices(invoicesFolder, processor);

        snapshotStore.Save(inventoryService.Items);
        watcher.StartMonitoring();

        Console.WriteLine("[System] Sync complete. Inventory data is ready.");

        var reportPresenter = new ConsoleReportPresenter(inventoryService, kpiService, reportWriter);
        reportPresenter.RunInteractiveMenu();

        snapshotStore.Save(inventoryService.Items);
    }

    private static async Task ImportHistoricalProducts(string productsFolder, FileProcessor processor)
    {
        Console.WriteLine($"[Files] Scanning product folder: {productsFolder}");

        if (!Directory.Exists(productsFolder))
        {
            Console.WriteLine($"[Error] Product folder '{productsFolder}' does not exist.");
            return;
        }

        var productFiles = Directory.GetFiles(productsFolder, "*.txt")
            .OrderBy(file => file)
            .ToList();

        Console.WriteLine($"[Files] Found {productFiles.Count} product file(s).");

        foreach (var file in productFiles)
        {
            await processor.ProcessProductFileAsync(file);
        }
    }

    private static async Task ImportHistoricalInvoices(string invoicesFolder, FileProcessor processor)
    {
        if (!Directory.Exists(invoicesFolder))
        {
            Console.WriteLine($"[Error] Invoice folder '{invoicesFolder}' does not exist.");
            return;
        }

        var invoiceFiles = Directory.GetFiles(invoicesFolder, "*.txt")
            .OrderBy(file => file);

        foreach (var file in invoiceFiles)
        {
            await processor.ProcessInvoiceFileAsync(file);
        }
    }

    private static string ResolveDataFolder(string childFolder)
    {
        var relativePath = Path.Combine("Data", childFolder);
        var candidates = new[]
        {
            Path.GetFullPath(relativePath),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath)),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", relativePath)),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "InventoryKpiSystem", relativePath)),
            Path.GetFullPath(Path.Combine("InventoryKpiSystem", relativePath))
        };

        return candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
    }

    private static string ResolveRuntimeRoot(string invoicesFolder)
    {
        var dataFolder = Directory.GetParent(invoicesFolder);
        return dataFolder?.Parent?.FullName ?? Directory.GetCurrentDirectory();
    }
}
