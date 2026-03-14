using System;
using System.Collections.Generic;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.Inventory
{
    public class InventoryState
    {
        public Dictionary<string, ProductInventory> Products { get; set; }

        public InventoryState()
        {
            Products = new Dictionary<string, ProductInventory>();
        }

        public void AddPurchase(string productId, int quantity, decimal unitCost, DateTime date)
        {
            if (!Products.ContainsKey(productId))
            {
                Products[productId] = new ProductInventory
                {
                    ProductId = productId
                };
            }

            // Cập nhật số lượng nhập và ngày nhập
            Products[productId].PurchasedQuantity += quantity;
            Products[productId].PurchaseDates.Add(date);

            // ĐÃ SỬA: Luôn luôn cập nhật giá vốn (UnitCost) từ hóa đơn nhập hàng mới nhất!
            // Dù là sản phẩm mới hay cũ thì khi nhập hàng cũng phải ghi nhận lại giá.
            Products[productId].UnitCost = unitCost;
        }

        public void AddSale(string productId, int quantity, DateTime date)
        {
            if (!Products.ContainsKey(productId))
            {
                Products[productId] = new ProductInventory
                {
                    ProductId = productId
                };
            }

            Products[productId].SoldQuantity += quantity;
            Products[productId].SaleDates.Add(date);
        }
    }
}