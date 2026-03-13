using System.Text.Json;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.Inventory;
using InventoryKpiSystem.Services.KPI;
using InventoryKpiSystem.Services.Reporting;

Console.WriteLine("Starting Inventory KPI System...");

// ---------------- PATH SETUP ----------------
var basePath = Directory.GetCurrentDirectory();

var productPath = Path.Combine(basePath, "Data", "product.txt");
var invoicesFolder = Path.Combine(basePath, "Data", "Invoices");

Console.WriteLine($"Product file: {productPath}");
Console.WriteLine($"Invoices folder: {invoicesFolder}");

// ---------------- LOAD PRODUCTS ----------------
Console.WriteLine("Loading products...");

var json = File.ReadAllText(productPath);

var response = JsonSerializer.Deserialize<ProductResponse>(json);

var products = response.Items;

Console.WriteLine($"Loaded {products.Count} products");

// ---------------- LOAD INVOICES ----------------
Console.WriteLine("Loading invoices...");

var invoiceFiles = Directory.GetFiles(invoicesFolder, "*.txt");

var allInvoices = new List<Invoice>();

foreach (var file in invoiceFiles)
{
    try
    {
        var invoiceJson = File.ReadAllText(file);

        var invoices = JsonSerializer.Deserialize<List<Invoice>>(invoiceJson);

        if (invoices != null)
        {
            allInvoices.AddRange(invoices);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load {file}: {ex.Message}");
    }
}

Console.WriteLine($"Loaded {allInvoices.Count} invoices");

// ---------------- BUILD INVENTORY ----------------
var inventoryState = new InventoryState();

// load initial products
foreach (var product in products)
{
    inventoryState.Products[product.ItemCode] = product;
}

// ---------------- UPDATE INVENTORY ----------------
Console.WriteLine("Processing invoices...");

var updater = new InventoryUpdater(inventoryState);
updater.ProcessInvoices(allInvoices);

// ---------------- CALCULATE KPI ----------------
Console.WriteLine("Calculating KPI...");

var kpiEngine = new KpiEngine();

var inventories = inventoryState.Products.Values.ToList();

var report = new KpiReport
{
    TotalSkus = kpiEngine.GetTotalSkus(inventories),
    InventoryValue = kpiEngine.GetStockValue(inventories),
    OutOfStockItems = kpiEngine.GetOutOfStockItems(inventories),
    AverageDailySales = kpiEngine.GetAverageDailySales(inventories, 30),
    AverageInventoryAge = kpiEngine.GetAverageInventoryAge(inventories)
};

// ---------------- GENERATE REPORT ----------------
Console.WriteLine("Generating report...");

var reportGenerator = new ReportGenerator();
reportGenerator.GenerateReport(report);

Console.WriteLine("KPI Report generated successfully!");