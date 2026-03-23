using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.KPI;

public class KpiEngineTests
{
    [Fact]
    public void GetStockValue_ShouldReturnCorrectValue()
    {
        var inventories = new List<ProductInventory>
        {
            new ProductInventory { UnitCost = 10, PurchasedQuantity = 5, SoldQuantity = 0 }, // 10 * 5 = 50
            new ProductInventory { UnitCost = 20, PurchasedQuantity = 2, SoldQuantity = 0 }  // 20 * 2 = 40
        };

        var engine = new KpiEngine();

        var result = engine.GetStockValue(inventories);

        result.Should().Be(90);
    }

    [Fact]
    public void GetStockValue_ShouldReturnZero_WhenEmpty()
    {
        var inventories = new List<ProductInventory>();
        var engine = new KpiEngine();

        var result = engine.GetStockValue(inventories);

        result.Should().Be(0);
    }
}