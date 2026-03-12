using System;
using System.Collections.Generic;
using System.Linq;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.KPI
{
    public class KpiEngine : IKpiCalculator
    {

        public int GetTotalSkus(List<ProductInventory> inventories)
        {
            return inventories
                .Select(i => i.ProductId)
                .Distinct()
                .Count();
        }

        public decimal GetStockValue(List<ProductInventory> inventories)
        {
            return inventories
                .Sum(i => (i.PurchasedQuantity - i.SoldQuantity) * i.UnitCost);
        }


        public int GetOutOfStockItems(List<ProductInventory> inventories)
        {
            return inventories
                .Count(i => (i.PurchasedQuantity - i.SoldQuantity) <= 0);
        }


        public double GetAverageDailySales(List<ProductInventory> inventories, int numberOfDays)
        {
            var totalSold = inventories.Sum(i => i.SoldQuantity);

            if (numberOfDays == 0)
                return 0;

            return (double)totalSold / numberOfDays;
        }


        public double GetAverageInventoryAge(List<ProductInventory> inventories)
        {
            var unsoldItems = inventories
                .Where(i => (i.PurchasedQuantity - i.SoldQuantity) > 0)
                .ToList();

            if (!unsoldItems.Any())
                return 0;

            return unsoldItems
                .Average(i => i.PurchaseDates.Any() 
                    ? i.PurchaseDates.Average(date => (DateTime.Now - date).TotalDays) 
                    : 0);
        }

    }
}