using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.DTOs; // Thêm thư viện chứa KpiResult
using InventoryKpiSystem.Services.Inventory;
using InventoryKpiSystem.Services.KPI;
using System.Threading.Tasks;
using InventoryKpiSystem.Services.FileProcessing;
using InventoryKpiSystem.Services.FileMonitoring;
using InventoryKpiSystem.Services.Idempotency;
using InventoryKpiSystem.Services.Reporting; // Thêm thư viện chứa ReportGenerator

Console.WriteLine("=================================================");
Console.WriteLine("    INVENTORY KPI SYSTEM - REAL-TIME SERVICE");
Console.WriteLine("=================================================");

var basePath = Directory.GetCurrentDirectory();
var productPath = Path.Combine(basePath, "Data", "product.txt");
var invoicesFolder = Path.Combine(basePath, "Data", "Invoices");
var productsFolder = Path.Combine(basePath, "Data", "Product");

if (!Directory.Exists(invoicesFolder)) Directory.CreateDirectory(invoicesFolder);
if (!Directory.Exists(productsFolder)) Directory.CreateDirectory(productsFolder);

Console.WriteLine($"\n[System] LƯU Ý: HÃY THẢ FILE MỚI VÀO MỘT TRONG HAI THƯ MỤC NÀY:");
Console.WriteLine($"👉 {invoicesFolder}\n");
Console.WriteLine($"👉 {productsFolder}\n");

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var inventoryState = new InventoryState();
var kpiEngine = new KpiEngine();

// KHỞI TẠO CÁC DỊCH VỤ ĐÃ ĐƯỢC CHIA TÁCH
var fileRegistry = new ProcessedFileRegistry();
var fileProcessor = new FileProcessor(inventoryState, jsonOptions, fileRegistry);

// 1. Khởi tạo ReportGenerator
var reportGenerator = new ReportGenerator();

// Hàm in báo cáo (Đã được nâng cấp để dùng ReportGenerator và KpiResult)
Action printReport = () =>
{
    var inventories = inventoryState.GetAllInventory();

    // 2. Đóng gói dữ liệu tính toán được vào DTO KpiResult
    var kpiResult = new KpiResult
    {
        GeneratedAt = DateTime.Now,
        TotalSkus = kpiEngine.GetTotalSkus(inventories),
        InventoryValue = kpiEngine.GetStockValue(inventories),
        OutOfStockItems = kpiEngine.GetOutOfStockItems(inventories),
        AverageDailySales = kpiEngine.GetAverageDailySales(inventories),
        AverageInventoryAge = kpiEngine.GetAverageInventoryAge(inventories)
    };

    // 3. Giao nhiệm vụ in màn hình và xuất file JSON cho ReportGenerator
    reportGenerator.GenerateReport(kpiResult);
};

// 1. NẠP DỮ LIỆU SẢN PHẨM BAN ĐẦU
Console.WriteLine("[System] Loading product catalog...");
if (File.Exists(productPath))
{
    var json = File.ReadAllText(productPath);
    var response = JsonSerializer.Deserialize<ProductResponse>(json, jsonOptions);
    if (response?.Items != null)
    {
        foreach (var p in response.Items)
            inventoryState.Products[p.ItemCode] = p;
    }
}

// 2. NẠP CÁC HÓA ĐƠN LỊCH SỬ TỪ ĐẦU
Console.WriteLine("[System] Loading historical invoices...");
foreach (var file in Directory.GetFiles(invoicesFolder, "*.txt"))
{
    await fileProcessor.ProcessInvoiceFileAsync(file);
}

// In báo cáo khởi tạo
printReport();

// 3. KÍCH HOẠT THEO DÕI REAL-TIME
using var monitorService = new FileMonitorService(invoicesFolder, productsFolder, fileProcessor, printReport);
monitorService.StartMonitoring();

Console.WriteLine("[System] Press ENTER to stop the service...\n");
Console.ReadLine();