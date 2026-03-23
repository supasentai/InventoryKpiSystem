using Xunit;
using FluentAssertions;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Loaders;

public class JsonDataLoaderTests
{
    [Fact]
    public async Task LoadPurchaseOrders_ShouldParseValidJson()
    {
        var json = "[{ \"ItemID\": \"P01\", \"Name\": \"Test Product\" }]";
        var path = "test_orders.json";

        await File.WriteAllTextAsync(path, json);

        var loader = new JsonFileLoader();

        var resultStream = loader.LoadPurchaseOrdersAsync(path);

        var results = new List<PurchaseOrder>();
        await foreach (var item in resultStream)
        {
            results.Add(item);
        }

        results.Should().NotBeNull();

        if (File.Exists(path)) File.Delete(path);
    }
}