using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryKpiSystem.Services.FileMonitoring
{
    public class FileMonitorService : BackgroundService
    {
        private readonly ILogger<FileMonitorService> _logger;
        private readonly ChannelWriter<string> _fileQueueWriter;
        private FileSystemWatcher? _invoiceWatcher;
        private FileSystemWatcher? _purchaseOrderWatcher;


        private readonly string InvoicesPath = Path.Combine(AppContext.BaseDirectory, "data", "invoices");
        private readonly string PurchaseOrdersPath = Path.Combine(AppContext.BaseDirectory, "data", "purchase-orders");

        public FileMonitorService(
            ILogger<FileMonitorService> logger,
            ChannelWriter<string> fileQueueWriter)
        {
            _logger = logger;
            _fileQueueWriter = fileQueueWriter;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("File Monitor Service is starting.");

            SetupWatcher(ref _invoiceWatcher, InvoicesPath);
            SetupWatcher(ref _purchaseOrderWatcher, PurchaseOrdersPath);

          
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("File Monitor Service is stopping gracefully.");
            }
        }

        private void SetupWatcher(ref FileSystemWatcher? watcher, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogInformation($"Created directory: {path}");
            }

            watcher = new FileSystemWatcher(path, "*.json")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            watcher.Created += OnFileDetected;
            watcher.Renamed += OnFileDetected;

            _logger.LogInformation($"Started monitoring: {path}");
        }

        private void OnFileDetected(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"Detected file change ({e.ChangeType}): {e.FullPath}");

            Task.Run(async () =>
            {
                try
                {
                    await ProcessAndEnqueueFileAsync(e.FullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Critical error processing file: {e.FullPath}");
                }
            });
        }

        private async Task ProcessAndEnqueueFileAsync(string filePath)
        {
            if (await IsFileReadyAsync(filePath))
            {
                await _fileQueueWriter.WriteAsync(filePath);
                _logger.LogInformation($"Successfully pushed to queue: {filePath}");
            }
            else
            {
                _logger.LogWarning($"Failed to enqueue. File might be locked, empty, or unreadable: {filePath}");
            }
        }

        private async Task<bool> IsFileReadyAsync(string filePath, int maxRetries = 10, int delayMilliseconds = 500)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    if (stream.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogDebug($"File is currently 0 bytes, waiting for data... ({filePath})");
                    }
                }
                catch (IOException)
                {
                    _logger.LogDebug($"File locked, retrying {i + 1}/{maxRetries}... ({filePath})");
                }

                await Task.Delay(delayMilliseconds);
            }

            return false;
        }

        public override void Dispose()
        {
            _invoiceWatcher?.Dispose();
            _purchaseOrderWatcher?.Dispose();
            base.Dispose();
        }
    }
}