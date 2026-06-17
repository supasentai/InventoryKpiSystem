using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Application.Services;

public class KpiService : IKpiService
{
    public int GetTotalSkus(List<InventoryItem> inventories)
    {
        return inventories.Count(item => item.PurchaseBatches.Any() || item.TotalSoldQuantity > 0);
    }

    public decimal GetStockValue(List<InventoryItem> inventories)
    {
        return inventories.Sum(item => item.TotalStockValue);
    }

    public int GetOutOfStockItems(List<InventoryItem> inventories)
    {
        return inventories.Count(item =>
            (item.PurchaseBatches.Any() || item.TotalSoldQuantity > 0) &&
            item.QuantityOnHand <= 0);
    }

    public double GetAverageDailySales(List<InventoryItem> inventories)
    {
        var validSaleDates = inventories
            .SelectMany(item => item.SaleDates)
            .Where(date => date.Year > 2000)
            .ToList();

        if (!validSaleDates.Any())
        {
            return 0;
        }

        var firstSale = validSaleDates.Min();
        var lastSale = validSaleDates.Max();
        var salesDays = (lastSale - firstSale).TotalDays;

        if (salesDays < 1)
        {
            salesDays = 1;
        }

        var totalSold = inventories.Sum(item => item.TotalSoldQuantity);
        return totalSold / salesDays;
    }

    public double GetAverageInventoryAge(List<InventoryItem> inventories)
    {
        var inStockItems = inventories.Where(item => item.QuantityOnHand > 0).ToList();
        if (!inStockItems.Any())
        {
            return 0;
        }

        var allSaleDates = inventories
            .SelectMany(item => item.SaleDates)
            .Where(date => date.Year > 2000)
            .ToList();

        var reportDate = allSaleDates.Any() ? allSaleDates.Max() : DateTime.Now;

        return inStockItems.Average(item =>
        {
            var remainingBatches = item.PurchaseBatches
                .Where(batch => batch.RemainingQuantity > 0)
                .ToList();

            if (!remainingBatches.Any())
            {
                return 0;
            }

            var totalWeightedAge = remainingBatches.Sum(batch =>
                (reportDate - batch.PurchaseDate).TotalDays * batch.RemainingQuantity);

            return totalWeightedAge / item.QuantityOnHand;
        });
    }

    public KpiResult CreateSnapshot(List<InventoryItem> inventories)
    {
        return new KpiResult
        {
            GeneratedAt = DateTime.Now,
            TotalSkus = GetTotalSkus(inventories),
            InventoryValue = GetStockValue(inventories),
            OutOfStockItems = GetOutOfStockItems(inventories),
            AverageDailySales = GetAverageDailySales(inventories),
            AverageInventoryAge = GetAverageInventoryAge(inventories)
        };
    }
}
