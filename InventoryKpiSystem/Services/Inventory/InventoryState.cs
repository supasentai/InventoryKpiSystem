using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.Inventory
{
    public class InventoryState
    {
        private readonly string _snapshotFilePath = "inventory-snapshot.json";
        private readonly object _fileLock = new object();

        public ConcurrentDictionary<string, ProductInventory> Products { get; private set; } = new();

        public InventoryState()
        {
            LoadSnapshot();
        }

        public void AddPurchase(string itemCode, int quantity, decimal unitCost, DateTime date)
        {
            var product = Products.GetOrAdd(itemCode, id => new ProductInventory { ItemCode = id, ProductId = Guid.NewGuid().ToString() });
            lock (product)
            {
                product.PurchaseBatches.Add(new PurchaseBatch
                {
                    PurchaseDate = date,
                    UnitCost = unitCost,
                    InitialQuantity = quantity,
                    RemainingQuantity = quantity
                });
            }
        }

        public void AddSale(string itemCode, int quantity, DateTime date)
        {
            var product = Products.GetOrAdd(itemCode, id => new ProductInventory { ItemCode = id, ProductId = Guid.NewGuid().ToString() });
            lock (product)
            {
                product.TotalSoldQuantity += quantity;
                product.SaleDates.Add(date);

                int remainingToDeduct = quantity;
                var availableBatches = product.PurchaseBatches.Where(b => b.RemainingQuantity > 0).OrderBy(b => b.PurchaseDate).ToList();

                foreach (var batch in availableBatches)
                {
                    if (remainingToDeduct <= 0) break;
                    if (batch.RemainingQuantity >= remainingToDeduct)
                    {
                        batch.RemainingQuantity -= remainingToDeduct;
                        remainingToDeduct = 0;
                    }
                    else
                    {
                        remainingToDeduct -= batch.RemainingQuantity;
                        batch.RemainingQuantity = 0;
                    }
                }
            }
        }

        public List<ProductInventory> GetAllInventory() => Products.Values.ToList();

        public void SaveSnapshot()
        {
            lock (_fileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var snapshotData = Products.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    string json = JsonSerializer.Serialize(snapshotData, options);
                    File.WriteAllText(_snapshotFilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Lỗi Snapshot] Không thể lưu trạng thái kho: {ex.Message}");
                }
            }
        }

        private void LoadSnapshot()
        {
            if (!File.Exists(_snapshotFilePath)) return;

            lock (_fileLock)
            {
                try
                {
                    string json = File.ReadAllText(_snapshotFilePath);
                    var snapshotData = JsonSerializer.Deserialize<Dictionary<string, ProductInventory>>(json);

                    if (snapshotData != null)
                    {
                        Products = new ConcurrentDictionary<string, ProductInventory>(snapshotData);
                        Console.WriteLine($"[System] Đã nạp thành công Snapshot cũ! Khôi phục {Products.Count} sản phẩm vào RAM.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Lỗi Snapshot] Không thể nạp trạng thái kho: {ex.Message}");
                }
            }
        }
    }
}