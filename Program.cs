using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.Inventory;
using InventoryKpiSystem.Services.KPI;
using System.Threading.Tasks;
using InventoryKpiSystem.Services.FileProcessing;
using InventoryKpiSystem.Services.FileMonitoring;
using InventoryKpiSystem.Services.Idempotency;

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
// 1. Thêm Registry để lưu lịch sử file (chống trùng lặp - Idempotency)
var fileRegistry = new ProcessedFileRegistry();

// 2. Truyền Registry vào FileProcessor
var fileProcessor = new FileProcessor(inventoryState, jsonOptions, fileRegistry);

// Hàm in báo cáo (Gói gọn vào 1 Action để truyền qua cho MonitorService)
Action printReport = () =>
{
    // Sử dụng hàm GetAllInventory() đã được tối ưu cho ConcurrentDictionary
    var inventories = inventoryState.GetAllInventory();

    Console.WriteLine("=================================");
    Console.WriteLine($" KPI REPORT (As of {DateTime.Now:HH:mm:ss})");
    Console.WriteLine("=================================");
    Console.WriteLine($"Total SKUs:             {kpiEngine.GetTotalSkus(inventories):N0}");
    Console.WriteLine($"Inventory Value:        ${kpiEngine.GetStockValue(inventories):N2}");
    Console.WriteLine($"Out-of-Stock Items:     {kpiEngine.GetOutOfStockItems(inventories):N0}");
    Console.WriteLine($"Average Daily Sales:    {kpiEngine.GetAverageDailySales(inventories):N2} units/day");
    Console.WriteLine($"Average Inventory Age:  {kpiEngine.GetAverageInventoryAge(inventories):N2} days");
    Console.WriteLine("=================================");
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
            inventoryState.Products[p.ItemCode] = p; // Cập nhật an toàn vào ConcurrentDictionary
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