using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using InventoryKpiSystem.Services.FileProcessing;
using InventoryKpiSystem.Services.Inventory;

namespace InventoryKpiSystem.Services.FileMonitoring
{
    public class FileMonitorService
    {
        private readonly string _invoicesFolder;
        private readonly string _productsFolder;
        private readonly FileProcessor _processor;
        private readonly FileProcessingQueue _queue;
        private readonly Action _onFileProcessed;
        private readonly InventoryState _state;

        private readonly ConcurrentDictionary<string, DateTime> _recentFiles = new();

        public FileMonitorService(
            string invoicesFolder,
            string productsFolder,
            FileProcessor processor,
            FileProcessingQueue queue,
            Action onFileProcessed,
            InventoryState state)
        {
            _invoicesFolder = invoicesFolder;
            _productsFolder = productsFolder;
            _processor = processor;
            _queue = queue;
            _onFileProcessed = onFileProcessed;
            _state = state;
        }

        public void StartMonitoring()
        {
            SetupWatcher(_invoicesFolder);
            SetupWatcher(_productsFolder);

            Task.Run(ProcessQueueAsync);

            Console.WriteLine("[System] Radar canh gác file mới đang hoạt động ngầm (Sử dụng Channel)...");
        }

        private void SetupWatcher(string folderPath)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var watcher = new FileSystemWatcher(folderPath, "*.txt")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            watcher.Created += (s, e) => OnFileDetected(e.FullPath);
            watcher.Changed += (s, e) => OnFileDetected(e.FullPath);

            watcher.EnableRaisingEvents = true;
        }

        private void OnFileDetected(string filePath)
        {
            if (_recentFiles.TryGetValue(filePath, out var lastTime))
            {
                if ((DateTime.Now - lastTime).TotalSeconds < 2) return;
            }

            _recentFiles[filePath] = DateTime.Now;


            string currentTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            string fileName = Path.GetFileName(filePath);

            Console.WriteLine($"\n=======================================================");
            Console.WriteLine($"[📥 ĐÃ THÊM FILE MỚI] {fileName}");
            Console.WriteLine($"[⏰ THỜI GIAN]        {currentTime}");
            Console.WriteLine($"=======================================================");
            Console.WriteLine("Hệ thống đang xử lý, vui lòng đợi trong giây lát...");

            // Bơm vào ống dẫn Channel
            _ = _queue.EnqueueFileAsync(filePath);
        }

        private async Task ProcessQueueAsync()
        {
            await foreach (var filePath in _queue.Reader.ReadAllAsync())
            {
                var fileName = Path.GetFileName(filePath);
                try
                {
                    await Task.Delay(1000);

                    if (fileName.Contains("product", StringComparison.OrdinalIgnoreCase))
                    {
                        await _processor.ProcessProductFileAsync(filePath);
                    }
                    else
                    {
                        await _processor.ProcessInvoiceFileAsync(filePath);
                    }

                    _state.SaveSnapshot();

                    Console.WriteLine($"[✅ XỬ LÝ XONG] Dữ liệu từ '{fileName}' đã được nạp và cập nhật Snapshot!");

                    _onFileProcessed?.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[🚨 LỖI ĐỌC FILE NGẦM] {fileName}: {ex.Message}");
                    _onFileProcessed?.Invoke();
                }
            }
        }
    }
}