using FluentAssertions;
using Inventory.Application.Services;

namespace Inventory.Application.Tests;

public class KpiServiceTests
{
    [Fact]
    public void GetStockValue_ShouldCalculateInventoryValueCorrectly()
    {
        var inventoryService = new InventoryService(new FifoCostingService());
        inventoryService.AddPurchase("A", 5, 10m, new DateTime(2024, 1, 1));
        inventoryService.AddPurchase("B", 2, 20m, new DateTime(2024, 1, 1));

        var result = new KpiService().GetStockValue(inventoryService.GetAllInventory());

        result.Should().Be(90m);
    }

    [Fact]
    public void GetOutOfStockItems_ShouldCountOnlyTouchedItemsWithNoStock()
    {
        var inventoryService = new InventoryService(new FifoCostingService());
        inventoryService.AddPurchase("A", 5, 10m, new DateTime(2024, 1, 1));
        inventoryService.AddSale("A", 5, new DateTime(2024, 1, 2));
        inventoryService.AddPurchase("B", 2, 20m, new DateTime(2024, 1, 1));

        var result = new KpiService().GetOutOfStockItems(inventoryService.GetAllInventory());

        result.Should().Be(1);
    }
}
