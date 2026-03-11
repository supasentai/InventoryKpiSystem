using System.Collections.Generic;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.KPI
{
    public interface IKpiCalculator
    {
        int GetTotalSkus(List<ProductInventory> inventories);

        decimal GetStockValue(List<ProductInventory> inventories);

        int GetOutOfStockItems(List<ProductInventory> inventories);

        double GetAverageDailySales(List<ProductInventory> inventories, int numberOfDays);

        double GetAverageInventoryAge(List<ProductInventory> inventories);
    }
}