using FluentAssertions;
using Inventory.Application.Services;

namespace Inventory.Application.Tests;

public class InventoryServiceTests
{
    [Fact]
    public void AddPurchase_ShouldIncreaseStock()
    {
        var service = CreateService();

        service.AddPurchase("A", 10, 5m, new DateTime(2024, 1, 1));

        var product = service.GetAllInventory().Single();
        product.ItemCode.Should().Be("A");
        product.QuantityOnHand.Should().Be(10);
        product.TotalStockValue.Should().Be(50m);
    }

    [Fact]
    public void AddSale_ShouldDecreaseStockUsingFifo()
    {
        var service = CreateService();
        service.AddPurchase("A", 5, 10m, new DateTime(2024, 1, 1));
        service.AddPurchase("A", 5, 20m, new DateTime(2024, 1, 2));

        service.AddSale("A", 7, new DateTime(2024, 1, 3));

        var product = service.GetAllInventory().Single();
        product.QuantityOnHand.Should().Be(3);
        product.TotalSoldQuantity.Should().Be(7);
        product.PurchaseBatches[0].RemainingQuantity.Should().Be(0);
        product.PurchaseBatches[1].RemainingQuantity.Should().Be(3);
        product.TotalStockValue.Should().Be(60m);
    }

    [Fact]
    public void AddSale_ShouldHandleSaleLargerThanStockSafely()
    {
        var service = CreateService();
        service.AddPurchase("A", 3, 10m, new DateTime(2024, 1, 1));

        service.AddSale("A", 10, new DateTime(2024, 1, 2));

        var product = service.GetAllInventory().Single();
        product.QuantityOnHand.Should().Be(0);
        product.TotalStockValue.Should().Be(0m);
        product.TotalSoldQuantity.Should().Be(10);
    }

    private static InventoryService CreateService()
    {
        return new InventoryService(new FifoCostingService());
    }
}
