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
            return inventories.Count(i => i.PurchasedQuantity > 0 || i.SoldQuantity > 0);
        }

        public decimal GetStockValue(List<ProductInventory> inventories)
        {
            return inventories.Sum(i => 
                Math.Max(0, i.PurchasedQuantity - i.SoldQuantity) * i.UnitCost);
        }

        public int GetOutOfStockItems(List<ProductInventory> inventories)
        {
            return inventories.Count(i => 
                (i.PurchasedQuantity > 0 || i.SoldQuantity > 0) && // Phải là hàng đang kinh doanh
                (i.PurchasedQuantity - i.SoldQuantity) <= 0);      // Và lượng tồn kho chạm đáy (<=0)
        }

        public double GetAverageDailySales(List<ProductInventory> inventories)
        {
            
            var validSaleDates = inventories
                .SelectMany(i => i.SaleDates)
                .Where(d => d.Year > 2000)
                .ToList();
            
            if (!validSaleDates.Any()) return 0;

            // Tính số ngày bán hàng (Sales Days) chuẩn xác
            var firstSale = validSaleDates.Min();
            var lastSale = validSaleDates.Max();
            var salesDays = (lastSale - firstSale).TotalDays;

            if (salesDays < 1) salesDays = 1;

            var totalSold = inventories.Sum(i => i.SoldQuantity);
            return totalSold / salesDays;
        }

        public double GetAverageInventoryAge(List<ProductInventory> inventories)
        {
            var unsoldItems = inventories.Where(i => (i.PurchasedQuantity - i.SoldQuantity) > 0).ToList();
            if (!unsoldItems.Any()) return 0;


            var currentDate = DateTime.Now;

            return unsoldItems.Average(i => 
            {
                var validPurchases = i.PurchaseDates.Where(d => d.Year > 2000).ToList();
                
                return validPurchases.Any() 
                    ? validPurchases.Average(date => (currentDate - date).TotalDays) 
                    : 0;
            });
        }
    }
}