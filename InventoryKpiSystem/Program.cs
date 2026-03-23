using System;
using System.IO; // Bổ sung thư viện đọc file
using System.Text.Json;
using System.Threading.Tasks; // Bổ sung thư viện chạy đa luồng
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.FileMonitoring;
using InventoryKpiSystem.Services.FileProcessing;
using InventoryKpiSystem.Services.Idempotency;
using InventoryKpiSystem.Services.Inventory;
using InventoryKpiSystem.Services.KPI;

namespace InventoryKpiSystem
{
    class Program
    {
        // ĐÃ SỬA: Đổi 'void' thành 'async Task' để dùng được tính năng quét file
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== KHỞI ĐỘNG HỆ THỐNG KPI ERP ===");

            string invoicesFolder = "Data/Invoices";
            string productsFolder = "Data/Products";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var registry = new ProcessedFileRegistry();
            var inventoryState = new InventoryState();
            var processor = new FileProcessor(inventoryState, options, registry);
            var queue = new FileProcessingQueue(1000);

            var fileQueue = new InventoryKpiSystem.Services.FileProcessing.FileProcessingQueue();

            // 2. Định nghĩa hành động gọi lại Menu sau khi đọc xong
            Action onFileProcessed = () => Console.Write("\nChọn chức năng (0-3): ");

            // 3. Truyền đủ 6 món vũ khí vào cho Radar
            var monitorService = new InventoryKpiSystem.Services.FileMonitoring.FileMonitorService(
                invoicesFolder,
                productsFolder,
                processor,
                fileQueue,
                onFileProcessed,
                inventoryState
            );

            inventoryState.SaveSnapshot();
            monitorService.StartMonitoring();

            Console.WriteLine("\n[System] Đang đồng bộ hóa dữ liệu lịch sử (Strict Order)...");

            // 1. KIỂM TRA THƯ MỤC SẢN PHẨM CỰC KỲ KHẮT KHE
            Console.WriteLine($"[Radar] Đang quét thư mục: {productsFolder}...");
            if (Directory.Exists(productsFolder))
            {

                var productFiles = Directory.GetFiles(productsFolder).OrderBy(f => f).ToList();
                Console.WriteLine($"[Radar] Đã tìm thấy {productFiles.Count} file trong thư mục Products!");

                foreach (var file in productFiles)
                {
                    Console.WriteLine($"[Radar] Bắt đầu gọi hàm đọc file: {file}");
                    await processor.ProcessProductFileAsync(file);
                }
            }
            else
            {
                Console.WriteLine($"[🚨 LỖI NGHIÊM TRỌNG] Thư mục '{productsFolder}' KHÔNG TỒN TẠI hoặc sai đường dẫn!");
            }

            // 2. Nạp dữ liệu Giao dịch (Invoices) theo ĐÚNG THỨ TỰ THỜI GIAN
            if (Directory.Exists(invoicesFolder))
            {
                // Sắp xếp file theo tên (VD: invoice_01 sẽ chạy trước invoice_02)
                // Điều này cứu sống thuật toán FIFO khỏi nghịch lý "Bán trước khi Nhập"
                var invoiceFiles = Directory.GetFiles(invoicesFolder, "*.txt").OrderBy(f => f);
                foreach (var file in invoiceFiles)
                {
                    await processor.ProcessInvoiceFileAsync(file);
                }
            }

            // 3. Đã xử lý xong 100% dữ liệu cũ một cách hoàn hảo. 
            // Giờ mới lưu Snapshot và Bật Radar canh gác file mới.
            inventoryState.SaveSnapshot();
            monitorService.StartMonitoring();

            Console.WriteLine("[System] Đồng bộ hoàn tất! Dữ liệu đã sẵn sàng.");

            // ==========================================================
            var kpiEngine = new InventoryKpiSystem.Services.KPI.KpiEngine();

            // 2. Truyền nó vào cho ReportGenerator
            var reportGenerator = new InventoryKpiSystem.Services.Reporting.ReportGenerator(inventoryState, kpiEngine);

            // 3. Chạy Menu giao diện mới
            reportGenerator.RunInteractiveMenu();
        }
    }
}