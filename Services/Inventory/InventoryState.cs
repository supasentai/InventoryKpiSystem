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
                    ProductId = productId,
                    UnitCost = unitCost
                };
            }

            Products[productId].PurchasedQuantity += quantity;
            Products[productId].PurchaseDates.Add(date);
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
        }
    }
}