using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.Inventory;
using InventoryKpiSystem.Services.KPI;

Console.WriteLine("=================================================");
Console.WriteLine("    INVENTORY KPI SYSTEM - REAL-TIME SERVICE");
Console.WriteLine("=================================================");

var basePath = Directory.GetCurrentDirectory(); 

var productPath = Path.Combine(basePath, "Data", "product.txt");
var invoicesFolder = Path.Combine(basePath, "Data", "Invoices");

if (!Directory.Exists(invoicesFolder)) Directory.CreateDirectory(invoicesFolder);

// BỔ SUNG THÊM: In ra đường dẫn tuyệt đối để bạn biết chính xác mình cần thả file vào đâu
Console.WriteLine($"\n[System] LƯU Ý: HÃY THẢ FILE MỚI VÀO ĐÚNG THƯ MỤC NÀY:");
Console.WriteLine($"👉 {invoicesFolder}\n");

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var inventoryState = new InventoryState();
var kpiEngine = new KpiEngine();

// 1. NẠP DỮ LIỆU SẢN PHẨM BAN ĐẦU
Console.WriteLine("[System] Loading product catalog...");
if (File.Exists(productPath))
{
    var json = File.ReadAllText(productPath);
    var response = JsonSerializer.Deserialize<ProductResponse>(json, jsonOptions);
    if (response?.Items != null)
    {
        foreach (var p in response.Items)
        {
            inventoryState.Products[p.ItemCode] = p;
        }
    }
}

// 2. NẠP CÁC HÓA ĐƠN ĐÃ CÓ SẴN (Lịch sử)
Console.WriteLine("[System] Loading historical invoices...");
var existingFiles = Directory.GetFiles(invoicesFolder, "*.txt");
foreach (var file in existingFiles)
{
    ProcessInvoiceFile(file, inventoryState, jsonOptions);
}

// In báo cáo lần 1 (Dữ liệu lịch sử)
PrintReport(inventoryState, kpiEngine);

// 3. KÍCH HOẠT CHẾ ĐỘ THỜI GIAN THỰC (REAL-TIME MONITORING)
using var watcher = new FileSystemWatcher(invoicesFolder);
watcher.Filter = "*.txt"; // Giám sát các file txt/json mới
watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

// Sự kiện: Khi có file mới được thả vào thư mục
watcher.Created += (sender, e) =>
{
    Console.WriteLine($"\n[⚡ NEW FILE DETECTED] {e.Name} - Processing...");
    
    // Đọc và cập nhật file mới vào kho
    ProcessInvoiceFile(e.FullPath, inventoryState, jsonOptions);
    
    // Tự động in lại báo cáo KPI mới mà không cần restart
    PrintReport(inventoryState, kpiEngine);
};

// Bắt đầu lắng nghe
watcher.EnableRaisingEvents = true;

Console.WriteLine("\n[System] Real-time monitoring is ACTIVE.");
Console.WriteLine("[System] Drop a new invoice file into the 'Data/Invoices' folder to see live updates.");
Console.WriteLine("[System] Press ENTER to stop the service...\n");
Console.ReadLine(); // Giữ cho chương trình chạy vĩnh viễn cho đến khi bấm Enter

// ====================================================================
// CÁC HÀM XỬ LÝ PHỤ TRỢ (LOCAL FUNCTIONS)
// ====================================================================

// Hàm xử lý 1 file hóa đơn (có cơ chế chống khóa file khi copy)
static void ProcessInvoiceFile(string filePath, InventoryState state, JsonSerializerOptions options)
{
    // Thử đọc file tối đa 3 lần (đề phòng file đang được hệ thống khác copy vào chưa xong)
    for (int i = 0; i < 3; i++)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var wrapper = JsonSerializer.Deserialize<InvoiceResponse>(json, options);

            if (wrapper?.Invoices != null)
            {
                foreach (var invoice in wrapper.Invoices)
                {
                    foreach (var line in invoice.LineItems)
                    {
                        if (string.IsNullOrWhiteSpace(line.ItemCode) || !state.Products.ContainsKey(line.ItemCode)) 
                            continue;

                        if (invoice.Type == "ACCPAY")
                            state.AddPurchase(line.ItemCode, (int)line.Quantity, line.UnitAmount, invoice.Date);
                        else if (invoice.Type == "ACCREC")
                            state.AddSale(line.ItemCode, (int)line.Quantity, invoice.Date);
                    }
                }
            }
            break; // Đọc thành công thì thoát vòng lặp retry
        }
        catch (IOException)
        {
            Thread.Sleep(500); // Đợi 0.5s rồi thử đọc lại nếu file bị khóa
        }
        catch (Exception)
        {
            break; // Lỗi cú pháp JSON thì bỏ qua file này
        }
    }
}

// Hàm in báo cáo ra màn hình
static void PrintReport(InventoryState state, KpiEngine engine)
{
    var inventories = state.Products.Values.ToList();
    
    Console.WriteLine("=================================");
    Console.WriteLine($" KPI REPORT (As of {DateTime.Now:HH:mm:ss})");
    Console.WriteLine("=================================");
    Console.WriteLine($"Total SKUs:             {engine.GetTotalSkus(inventories):N0}");
    Console.WriteLine($"Inventory Value:        ${engine.GetStockValue(inventories):N2}");
    Console.WriteLine($"Out-of-Stock Items:     {engine.GetOutOfStockItems(inventories):N0}");
    Console.WriteLine($"Average Daily Sales:    {engine.GetAverageDailySales(inventories):N2} units/day");
    Console.WriteLine($"Average Inventory Age:  {engine.GetAverageInventoryAge(inventories):N2} days");
    Console.WriteLine("=================================");
}