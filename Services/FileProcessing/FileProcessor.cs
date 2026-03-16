using System;
using System.Collections.Generic; // Để dùng HashSet
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks; // Để dùng Async/Await
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.Inventory;

namespace InventoryKpiSystem.Services.FileProcessing
{
    public class FileProcessor
    {
        private readonly InventoryState _state;
        private readonly JsonSerializerOptions _options;
        
        // BỔ SUNG CƠ CHẾ CHỐNG TRÙNG FILE (Prevent duplicate processing)
        private readonly HashSet<string> _processedFiles = new();

        public FileProcessor(InventoryState state, JsonSerializerOptions options)
        {
            _state = state;
            _options = options;
        }

        // BỔ SUNG CƠ CHẾ BẤT ĐỒNG BỘ (Async File I/O)
        public async Task ProcessInvoiceFileAsync(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            // Kiểm tra xem file này đã từng được đọc chưa? Nếu rồi thì bỏ qua ngay!
            if (_processedFiles.Contains(fileName))
            {
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // Dùng ReadAllTextAsync thay vì ReadAllText
                    var json = await File.ReadAllTextAsync(filePath);
                    var wrapper = JsonSerializer.Deserialize<InvoiceResponse>(json, _options);

                    if (wrapper?.Invoices != null)
                    {
                        foreach (var invoice in wrapper.Invoices)
                        {
                            foreach (var line in invoice.LineItems)
                            {
                                if (string.IsNullOrWhiteSpace(line.ItemCode) || !_state.Products.ContainsKey(line.ItemCode)) 
                                    continue;

                                if (invoice.Type == "ACCPAY")
                                    _state.AddPurchase(line.ItemCode, (int)line.Quantity, line.UnitAmount, invoice.Date);
                                else if (invoice.Type == "ACCREC")
                                    _state.AddSale(line.ItemCode, (int)line.Quantity, invoice.Date);
                            }
                        }
                    }
                    
                    // Ghi nhớ lại file này đã xử lý thành công để lần sau không đọc lại nữa
                    _processedFiles.Add(fileName);
                    break; 
                }
                catch (IOException)
                {
                    await Task.Delay(500); // Dùng Task.Delay thay cho Thread.Sleep
                }
                catch (Exception)
                {
                    break; 
                }
            }
        }
        public async Task ProcessProductFileAsync(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (_processedFiles.Contains(fileName)) return;

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
                            // Cập nhật sản phẩm cũ hoặc thêm mới sản phẩm vào kho
                            _state.Products[p.ItemCode] = p;
                        }
                    }
                    
                    _processedFiles.Add(fileName);
                    break;
                }
                catch (IOException)
                {
                    await Task.Delay(500);
                }
                catch (Exception)
                {
                    break; // Lỗi cú pháp JSON thì bỏ qua
                }
            }
        }
    }
}