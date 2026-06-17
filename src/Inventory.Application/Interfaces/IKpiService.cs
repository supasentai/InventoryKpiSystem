using Inventory.Application.DTOs;
using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IKpiService
{
    int GetTotalSkus(List<InventoryItem> inventories);

    decimal GetStockValue(List<InventoryItem> inventories);

    int GetOutOfStockItems(List<InventoryItem> inventories);

    double GetAverageDailySales(List<InventoryItem> inventories);

    double GetAverageInventoryAge(List<InventoryItem> inventories);

    KpiResult CreateSnapshot(List<InventoryItem> inventories);
}
