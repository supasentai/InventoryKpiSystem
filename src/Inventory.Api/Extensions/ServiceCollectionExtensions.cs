using Inventory.Api.Services;
using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Inventory.Infrastructure.FileParsing;
using Inventory.Infrastructure.Json;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Repositories;
using Inventory.Infrastructure.Persistence.Seed;
using Inventory.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;

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
        IConfiguration configuration,
        string contentRootPath)
    {
        var dataPaths = ResolveDataPaths(contentRootPath);
        var connectionString = configuration.GetConnectionString("InventoryDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'InventoryDb' is not configured.");
        }

        services.AddSingleton(dataPaths);
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<InventoryDatabaseSeeder>();
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
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IDatabaseHealthChecker, DatabaseHealthChecker>();
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
