using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.Inventory;
using InventoryKpiSystem.Services.Idempotency;

namespace InventoryKpiSystem.Services.FileProcessing
{
    public class FileProcessor
    {
        private readonly InventoryState _state;
        private readonly JsonSerializerOptions _options;
        private readonly ProcessedFileRegistry _registry;

        // Tiêm Registry vào qua Constructor
        public FileProcessor(InventoryState state, JsonSerializerOptions options, ProcessedFileRegistry registry)
        {
            _state = state;
            _options = options;
            _registry = registry;
        }

        public async Task ProcessInvoiceFileAsync(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            // 1. CHỐNG TRÙNG LẶP: Đã xử lý rồi thì bỏ qua
            if (_registry.IsFileProcessed(fileName)) return;

            for (int i = 0; i < 3; i++) // Cơ chế Retry nếu file đang bị hệ thống khóa
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var response = JsonSerializer.Deserialize<InvoiceResponse>(json, _options);

                    if (response?.Invoices != null)
                    {
                        foreach (var invoice in response.Invoices)
                        {
                            if (invoice.LineItems == null) continue;

                            foreach (var line in invoice.LineItems)

                            {
                                if (string.IsNullOrWhiteSpace(line.ItemCode)) continue;

                                int qty = (int)line.Quantity;

                                // 2. PHÂN LOẠI INVOICE
                                if (invoice.Type == "ACCPAY")
                                {
                                    _state.AddPurchase(line.ItemCode, qty, line.UnitAmount, invoice.Date);
                                }
                                else if (invoice.Type == "ACCREC")
                                {
                                    _state.AddSale(line.ItemCode, qty, invoice.Date);
                                }
                            }
                        }
                    }

                    // 3. LƯU LỊCH SỬ
                    _registry.MarkAsProcessed(fileName);
                    break;
                }
                catch (IOException)
                {
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Lỗi JSON Invoice] File {fileName}: {ex.Message}");
                    break;
                }
            }
        }

        public async Task ProcessProductFileAsync(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (_registry.IsFileProcessed(fileName)) return;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var response = JsonSerializer.Deserialize<ProductResponse>(json, _options);

                    if (response?.Items != null)
                    {
                        foreach (var p in response.Items)
                        {
                            var existingProduct = _state.Products.GetOrAdd(p.ItemCode, p);
                            lock (existingProduct)
                            {
                                existingProduct.Name = p.Name;
                                existingProduct.ProductId = p.ProductId;
                            }
                        }
                    }

                    _registry.MarkAsProcessed(fileName);
                    break;
                }
                catch (IOException)
                {
                    await Task.Delay(500);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }
}