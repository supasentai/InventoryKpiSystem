using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Loaders
{
    public class JsonFileLoader : IJsonFileLoader
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonFileLoader()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
        }

        public async IAsyncEnumerable<PurchaseOrder> LoadPurchaseOrdersAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in LoadDataStreamAsync<PurchaseOrder>(filePath, cancellationToken))
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<Invoice> LoadInvoicesAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in LoadDataStreamAsync<Invoice>(filePath, cancellationToken))
            {
                yield return item;
            }
        }

        // Khôi phục lại toàn bộ cấu trúc xịn sò của bạn và thêm tính năng nhận diện JSON thông minh
        private async IAsyncEnumerable<T> LoadDataStreamAsync<T>(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken) where T : class
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[Warning] File not found: {filePath}");
                yield break;
            }

            var fileOptions = new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                BufferSize = 65536
            };

            FileStream? stream = null;

            // BƯỚC 1: Mở file (Bắt lỗi nhưng KHÔNG dùng yield ở đây)
            try
            {
                stream = File.Open(filePath, fileOptions);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[Error] IO Exception for {filePath}: {ex.Message} (File có thể đang bị khóa)");
                yield break;
            }

            // BƯỚC 2: Xử lý dữ liệu (Dùng try-finally được phép dùng yield)
            try
            {
                int firstByte = stream.ReadByte();
                if (firstByte == -1) yield break;

                stream.Position = 0;

                if (firstByte == '[')
                {
                    var asyncStream = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, _jsonOptions, cancellationToken);
                    var enumerator = asyncStream.GetAsyncEnumerator(cancellationToken);
                    
                    try
                    {
                        bool hasNext = true;
                        while (hasNext)
                        {
                            T? currentItem = null;
                            
                            // Bắt lỗi riêng cho dòng đọc JSON hiện tại
                            try
                            {
                                hasNext = await enumerator.MoveNextAsync();
                                if (hasNext) currentItem = enumerator.Current;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Error] Invalid data in {filePath}: {ex.Message}");
                                break;
                            }

                            // Đã đưa lệnh yield ra AN TOÀN khỏi khối catch!
                            if (hasNext && currentItem != null)
                            {
                                yield return currentItem;
                            }
                        }
                    }
                    finally
                    {
                        await enumerator.DisposeAsync();
                    }
                }
                else if (firstByte == '{')
                {
                    if (typeof(T) == typeof(Invoice))
                    {
                        var wrapper = await JsonSerializer.DeserializeAsync<InvoiceResponse>(stream, _jsonOptions, cancellationToken);
                        if (wrapper?.Invoices != null)
                        {
                            foreach (var item in wrapper.Invoices)
                            {
                                yield return (item as T)!;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (stream != null) await stream.DisposeAsync();
            }
        }
    }
}