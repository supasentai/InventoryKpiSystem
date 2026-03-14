using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InventoryKpiSystem.Loaders;
using InventoryKpiSystem.Services.Inventory;

namespace InventoryKpiSystem.Services.FileProcessing
{
    public class FileProcessor : BackgroundService
    {
        private readonly FileProcessingQueue _queue;
        private readonly IJsonFileLoader _loader;
        private readonly InventoryState _state;
        private readonly ILogger<FileProcessor> _logger;

        public FileProcessor(
            FileProcessingQueue queue,
            IJsonFileLoader loader,
            InventoryState state,
            ILogger<FileProcessor> logger)
        {
            _queue = queue;
            _loader = loader;
            _state = state;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("File Processor is starting and waiting for files...");

            await foreach (var filePath in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessSingleFileAsync(filePath, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to process file: {filePath}");
                }
            }
        }

        private async Task ProcessSingleFileAsync(string filePath, CancellationToken token)
        {
            string fileName = Path.GetFileName(filePath).ToLower();

            if (fileName.Contains("purchase_orders"))
            {
                _logger.LogInformation($"Processing Purchase Orders: {fileName}");
                await foreach (var po in _loader.LoadPurchaseOrdersAsync(filePath, token))
                {
                    _state.AddPurchase(po.ProductId, po.Quantity, po.UnitCost, po.PurchaseDate);
                }
                _logger.LogInformation($"Successfully updated inventory from: {fileName}");
            }
            else if (fileName.Contains("invoices"))
            {
                _logger.LogInformation($"Processing Invoices: {fileName}");
                await foreach (var inv in _loader.LoadInvoicesAsync(filePath, token))
                {
                    // LẶP QUA TỪNG SẢN PHẨM TRONG HÓA ĐƠN
                    foreach (var line in inv.LineItems)
                    {
                        if (inv.Type == "ACCPAY")
                        {
                            // ACCPAY = Purchase Order (Nhập kho)
                            _state.AddPurchase(
                                line.ItemCode,     // Lấy mã SP từ LineItems
                                (int)line.Quantity,     // Lấy số lượng từ LineItems
                                line.UnitAmount,   // Lấy giá từ LineItems
                                inv.Date           // Lấy ngày lập hóa đơn
                            );
                        }
                        else if (inv.Type == "ACCREC")
                        {
                            // ACCREC = Sales Invoice (Xuất kho)
                            _state.AddSale(
                                line.ItemCode,     // Lấy mã SP từ LineItems
                                (int)line.Quantity,     // Lấy số lượng từ LineItems
                                inv.Date           // Lấy ngày lập hóa đơn
                            );
                        }
                    }
                }
                _logger.LogInformation($"Successfully updated inventory from: {fileName}");
            }
            else
            {
                _logger.LogWarning($"Unrecognized file type, skipping: {fileName}");
            }
        }
    }
}