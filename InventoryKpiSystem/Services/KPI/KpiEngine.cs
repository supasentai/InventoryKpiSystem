using System;
using System.Collections.Generic;
using System.Linq;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.KPI
{
    public class KpiEngine
    {
        public int GetTotalSkus(List<ProductInventory> inventories)
        {

            return inventories.Count(i => i.PurchaseBatches.Any() || i.TotalSoldQuantity > 0);
        }

        public decimal GetStockValue(List<ProductInventory> inventories)
        {

            return inventories.Sum(p => p.TotalStockValue);
        }

        public int GetOutOfStockItems(List<ProductInventory> inventories)
        {
            return inventories.Count(i =>
                (i.PurchaseBatches.Any() || i.TotalSoldQuantity > 0) && 
                i.QuantityOnHand <= 0); 
        }

        public double GetAverageDailySales(List<ProductInventory> inventories)
        {
            var validSaleDates = inventories
                .SelectMany(i => i.SaleDates)
                .Where(d => d.Year > 2000)
                .ToList();

            if (!validSaleDates.Any()) return 0;

            var firstSale = validSaleDates.Min();
            var lastSale = validSaleDates.Max();
            var salesDays = (lastSale - firstSale).TotalDays;

            if (salesDays < 1) salesDays = 1;

            var totalSold = inventories.Sum(i => i.TotalSoldQuantity);
            return totalSold / salesDays;
        }

        public double GetAverageInventoryAge(List<ProductInventory> inventories)
        {
            var inStockItems = inventories.Where(i => i.QuantityOnHand > 0).ToList();
            if (!inStockItems.Any()) return 0;

         
            var allSaleDates = inventories
                .SelectMany(i => i.SaleDates)
                .Where(d => d.Year > 2000)
                .ToList();

            DateTime reportDate = allSaleDates.Any() ? allSaleDates.Max() : DateTime.Now;

            return inStockItems.Average(item =>
            {
                var remainingBatches = item.PurchaseBatches.Where(b => b.RemainingQuantity > 0).ToList();
                if (!remainingBatches.Any()) return 0;

                double totalWeightedAge = remainingBatches.Sum(b =>
                    (reportDate - b.PurchaseDate).TotalDays * b.RemainingQuantity);

                return totalWeightedAge / item.QuantityOnHand;
            });
        }
    }
}