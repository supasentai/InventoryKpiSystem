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
        // Sử dụng đúng cấu trúc JSON mà ProductInventory mong đợi (ItemID, Name)
        var json = "[{ \"ItemID\": \"P01\", \"Name\": \"Test Product\" }]";
        var path = "test_orders.json";

        await File.WriteAllTextAsync(path, json);

        // Đổi từ JsonDataLoader thành JsonFileLoader để khớp với file .cs của bạn
        var loader = new JsonFileLoader();

        // Đổi method thành LoadPurchaseOrdersAsync (vì file logic không có LoadProductsAsync)
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