using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Inventory.Infrastructure.FileParsing;
using Inventory.Infrastructure.Json;
using Inventory.Infrastructure.Reporting;

namespace Inventory.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddOpenApi();

        return services;
    }

    public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
    {
        services.AddSingleton<IFifoCostingService, FifoCostingService>();
        services.AddSingleton<IImportService, ImportService>();
        services.AddSingleton<IKpiService, KpiService>();

        return services;
    }

    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        string contentRootPath)
    {
        var dataPaths = ResolveDataPaths(contentRootPath);

        services.AddSingleton(dataPaths);
        services.AddSingleton<IInventorySnapshotStore>(_ =>
            new JsonInventorySnapshotStore(Path.Combine(dataPaths.RuntimeRoot, "inventory-snapshot.json")));
        services.AddSingleton<IInventoryService>(provider =>
        {
            var fifoCostingService = provider.GetRequiredService<IFifoCostingService>();
            var snapshotStore = provider.GetRequiredService<IInventorySnapshotStore>();
            return new InventoryService(fifoCostingService, snapshotStore.Load());
        });
        services.AddSingleton<IProductFileReader, ProductFileReader>();
        services.AddSingleton<IInvoiceFileReader, InvoiceFileReader>();
        services.AddSingleton<IProcessedFileRegistry>(_ =>
            new ProcessedFileRegistry(Path.Combine(dataPaths.RuntimeRoot, "processed-files")));
        services.AddSingleton<IReportWriter>(_ =>
            new JsonReportWriter(Path.Combine(dataPaths.RuntimeRoot, "reports")));
        services.AddSingleton<FileProcessor>();

        return services;
    }

    private static ApiDataPaths ResolveDataPaths(string contentRootPath)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(currentDirectory, "InventoryKpiSystem")),
            Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "InventoryKpiSystem")),
            Path.GetFullPath(Path.Combine(contentRootPath, "InventoryKpiSystem"))
        };

        var runtimeRoot = candidates.FirstOrDefault(path =>
            Directory.Exists(Path.Combine(path, "Data", "Products")) &&
            Directory.Exists(Path.Combine(path, "Data", "Invoices"))) ?? candidates[0];

        return new ApiDataPaths(
            Path.Combine(runtimeRoot, "Data", "Products"),
            Path.Combine(runtimeRoot, "Data", "Invoices"),
            runtimeRoot);
    }
}

internal sealed record ApiDataPaths(
    string ProductsFolder,
    string InvoicesFolder,
    string RuntimeRoot);
