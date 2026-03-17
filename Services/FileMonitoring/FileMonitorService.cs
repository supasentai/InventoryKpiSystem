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
        }

        private FileSystemWatcher SetupWatcher(string folderPath)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var watcher = new FileSystemWatcher(folderPath)
            {
                Filter = "*.txt",
                // Mở rộng tai mắt: Lắng nghe thêm sự thay đổi kích thước và thời gian tạo (Chống OneDrive)
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,

                // Nâng cấp giáp: Tăng bộ đệm lên mức tối đa (64KB) để hứng đợt paste hàng chục file không bị rớt
                InternalBufferSize = 65536
            };

            // Bắt trọn mọi hành vi của file
            watcher.Created += OnFileDetected;
            watcher.Renamed += OnFileDetected;
            watcher.Changed += OnFileDetected; // Thêm sự kiện Changed

            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private async void OnFileDetected(object sender, FileSystemEventArgs e)
        {
            // BẮT BUỘC PHẢI CÓ TRY-CATCH CHO ASYNC VOID
            try
            {
                Console.WriteLine($"\n[⚡ FILE MỚI] {e.Name} - Đang xử lý...");

                if (e.FullPath.Contains("product", StringComparison.OrdinalIgnoreCase))
                {
                    await _processor.ProcessProductFileAsync(e.FullPath);
                }
                else
                {
                    await _processor.ProcessInvoiceFileAsync(e.FullPath);
                }

                _onFileProcessed?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cảnh báo Hệ thống] Bắt được lỗi: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _invoiceWatcher?.Dispose();
            _productWatcher?.Dispose();
        }
    }
}