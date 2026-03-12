using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.Inventory
{
    public class InventoryState
    {
        private readonly ConcurrentDictionary<string, ProductInventory> _inventory;        
        private readonly ConcurrentDictionary<DateTime, byte> _salesDays;

        public InventoryState()
        {
            _inventory = new ConcurrentDictionary<string, ProductInventory>();
            _salesDays = new ConcurrentDictionary<DateTime, byte>();
        }

        public void AddPurchase(string productId, int quantity, decimal unitCost, DateTime purchaseDate)
        {
            var product = _inventory.GetOrAdd(productId, id => new ProductInventory { ProductId = id });

            lock (product)
            {
                product.PurchasedQuantity += quantity; 
                product.UnitCost = unitCost;          
                product.PurchaseDates.Add(purchaseDate);
            }
        }

        public void AddSale(string productId, int quantity, DateTime invoiceDate)
        {
            _salesDays.TryAdd(invoiceDate.Date, byte.MinValue);

            var product = _inventory.GetOrAdd(productId, id => new ProductInventory { ProductId = id });

            lock (product)
            {
                product.SoldQuantity += quantity;
            }
        }

        public IReadOnlyDictionary<string, ProductInventory> GetAllInventory()
        {
            return _inventory;
        }

    }
}