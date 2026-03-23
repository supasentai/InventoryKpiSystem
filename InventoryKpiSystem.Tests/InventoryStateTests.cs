using Xunit;
using FluentAssertions;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services;
using InventoryKpiSystem.Services.Inventory;
using InventoryKpiSystem.Loaders;

public class InventoryStateTests
{
    [Fact]
    public void AddPurchase_ShouldIncreasePurchasedQuantity()
    {
        var state = new InventoryState();

        state.AddPurchase("A", 10, 5m, DateTime.Now);

        var product = state.GetAllInventory().First();

        product.PurchasedQuantity.Should().Be(10);
    }

    [Fact]
    public void AddSale_ShouldIncreaseSoldQuantity()
    {
        var state = new InventoryState();

        state.AddPurchase("A", 10, 5m, DateTime.Now);
        state.AddSale("A", 3, DateTime.Now);

        var product = state.GetAllInventory().First();

        product.SoldQuantity.Should().Be(3);
    }
}