using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.Inventory
{
    public class InventoryState
    {
        // Sử dụng ConcurrentDictionary để an toàn trong môi trường đa luồng
        public ConcurrentDictionary<string, ProductInventory> Products { get; } = new();

        public void AddPurchase(string itemCode, int quantity, decimal unitCost, DateTime date)
        {
            // Thao tác nguyên tử: Lấy ra hoặc tạo mới nếu chưa có
            var product = Products.GetOrAdd(itemCode, id => new ProductInventory { ItemCode = id, ProductId = Guid.NewGuid().ToString() });

            lock (product)
            {
                product.PurchasedQuantity += quantity;
                product.PurchaseDates.Add(date);
                product.UnitCost = unitCost;
            }
        }

        public void AddSale(string itemCode, int quantity, DateTime date)
        {
            var product = Products.GetOrAdd(itemCode, id => new ProductInventory { ItemCode = id, ProductId = Guid.NewGuid().ToString() });

            lock (product)
            {
                product.SoldQuantity += quantity;
                product.SaleDates.Add(date);
            }
        }

        // Cung cấp hàm lấy toàn bộ danh sách để KpiEngine tính toán
        public List<ProductInventory> GetAllInventory()
        {
            return Products.Values.ToList();
        }
    }
}