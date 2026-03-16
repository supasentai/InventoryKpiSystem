using System;
using System.IO;
using InventoryKpiSystem.Services.FileProcessing;

namespace InventoryKpiSystem.Services.FileMonitoring
{
    public class FileMonitorService : IDisposable
    {
        private readonly string _invoicesPath;
        private readonly string _productsPath;
        private readonly FileProcessor _processor;
        private readonly Action _onFileProcessed;
        
        // Dùng 2 watcher cho 2 thư mục khác nhau
        private FileSystemWatcher? _invoiceWatcher;
        private FileSystemWatcher? _productWatcher;

        public FileMonitorService(string invoicesPath, string productsPath, FileProcessor processor, Action onFileProcessed)
        {
            _invoicesPath = invoicesPath;
            _productsPath = productsPath;
            _processor = processor;
            _onFileProcessed = onFileProcessed;
        }

        public void StartMonitoring()
        {
            _invoiceWatcher = SetupWatcher(_invoicesPath);
            _productWatcher = SetupWatcher(_productsPath);

            Console.WriteLine("\n[System] Real-time monitoring is ACTIVE.");
            Console.WriteLine($"[System] Watching Invoices: {_invoicesPath}");
            Console.WriteLine($"[System] Watching Products: {_productsPath}");
        }

        // Hàm tạo lính canh đa năng
        private FileSystemWatcher SetupWatcher(string folderPath)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var watcher = new FileSystemWatcher(folderPath)
            {
                Filter = "*.txt",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            // BẮT ĐÚNG 2 SỰ KIỆN NHƯ YÊU CẦU CỦA RUBRIC
            watcher.Created += OnFileDetected;
            watcher.Renamed += OnFileDetected; 
            
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        // Dùng async void cho Event Handler
        private async void OnFileDetected(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"\n[⚡ NEW/RENAMED FILE DETECTED] {e.Name} - Processing...");
            
            // Phân loại xem file rớt vào thư mục nào để gọi đúng hàm xử lý
            if (e.FullPath.Contains("product", StringComparison.OrdinalIgnoreCase))
            {
                await _processor.ProcessProductFileAsync(e.FullPath);
            }
            else
            {
                await _processor.ProcessInvoiceFileAsync(e.FullPath);
            }
            
            // In báo cáo mới
            _onFileProcessed?.Invoke();
        }

        public void Dispose()
        {
            _invoiceWatcher?.Dispose();
            _productWatcher?.Dispose();
        }
    }
}