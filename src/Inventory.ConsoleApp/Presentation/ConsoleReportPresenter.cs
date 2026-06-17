using Inventory.Application.Interfaces;

namespace Inventory.ConsoleApp.Presentation;

public class ConsoleReportPresenter
{
    private readonly IInventoryService _inventoryService;
    private readonly IKpiService _kpiService;
    private readonly IReportWriter _reportWriter;

    public ConsoleReportPresenter(
        IInventoryService inventoryService,
        IKpiService kpiService,
        IReportWriter reportWriter)
    {
        _inventoryService = inventoryService;
        _kpiService = kpiService;
        _reportWriter = reportWriter;
    }

    public void RunInteractiveMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            Console.WriteLine("    INVENTORY KPI MENU");
            Console.WriteLine("=============================");
            Console.WriteLine("1. View KPI overview and export JSON");
            Console.WriteLine("2. Search product details");
            Console.WriteLine("3. Top 10 products by inventory value");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option (0-3): ");

            var choice = Console.ReadLine()?.Trim();
            switch (choice)
            {
                case "1":
                    PrintOverview();
                    break;
                case "2":
                    PrintProductDetails();
                    break;
                case "3":
                    PrintTopProducts();
                    break;
                case "0":
                    Console.WriteLine();
                    Console.WriteLine("Saving inventory state. Goodbye!");
                    return;
                default:
                    Console.WriteLine();
                    Console.WriteLine("Invalid option, please try again.");
                    break;
            }
        }
    }

    private void PrintOverview()
    {
        var allItems = _inventoryService.GetAllInventory();

        Console.WriteLine();
        Console.WriteLine("--- KPI OVERVIEW ---");
        Console.WriteLine($"- Total SKUs: {_kpiService.GetTotalSkus(allItems)}");
        Console.WriteLine($"- Total inventory value: {_kpiService.GetStockValue(allItems):C}");
        Console.WriteLine($"- Out-of-stock items: {_kpiService.GetOutOfStockItems(allItems)}");
        Console.WriteLine($"- Average daily sales: {_kpiService.GetAverageDailySales(allItems):F2} items/day");
        Console.WriteLine($"- Average inventory age: {_kpiService.GetAverageInventoryAge(allItems):F2} days");

        var path = _reportWriter.Write(_kpiService.CreateSnapshot(allItems));
        Console.WriteLine();
        Console.WriteLine($"[Success] Report exported to: {path}");
    }

    private void PrintProductDetails()
    {
        Console.WriteLine();
        Console.Write("Enter product ItemCode: ");
        var searchCode = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(searchCode))
        {
            Console.WriteLine("[Error] Product code cannot be empty.");
            return;
        }

        if (!_inventoryService.TryGetItem(searchCode, out var product) || product is null)
        {
            Console.WriteLine();
            Console.WriteLine($"[System] No product found with code '{searchCode}'.");
            return;
        }

        var age = _kpiService.GetAverageInventoryAge([product]);
        var unitValue = product.QuantityOnHand > 0
            ? product.TotalStockValue / product.QuantityOnHand
            : 0;

        Console.WriteLine();
        Console.WriteLine($"--- PRODUCT DETAIL: {searchCode} ---");
        Console.WriteLine($"- Name: {(string.IsNullOrEmpty(product.Name) ? "[Name not updated]" : product.Name)}");
        Console.WriteLine($"- Product code: {product.ItemCode}");
        Console.WriteLine($"- Unit value: {unitValue:C}");
        Console.WriteLine($"- Quantity on hand: {product.QuantityOnHand:N0}");
        Console.WriteLine($"- Inventory value: {product.TotalStockValue:C}");
        Console.WriteLine($"- Inventory age: {age:F2} days");
    }

    private void PrintTopProducts()
    {
        Console.WriteLine();
        Console.WriteLine("=== TOP 10 PRODUCTS BY INVENTORY VALUE ===");

        var topProducts = _inventoryService.Items.Values
            .Where(product => product.QuantityOnHand > 0 && product.TotalStockValue > 0)
            .OrderByDescending(product => product.TotalStockValue)
            .Take(10)
            .ToList();

        if (!topProducts.Any())
        {
            Console.WriteLine("Inventory is empty or no products have inventory value.");
            return;
        }

        var rank = 1;
        foreach (var product in topProducts)
        {
            var age = _kpiService.GetAverageInventoryAge([product]);
            var unitValue = product.TotalStockValue / product.QuantityOnHand;

            Console.WriteLine();
            Console.WriteLine($"[{rank}] Code: {product.ItemCode} | Name: {(string.IsNullOrEmpty(product.Name) ? "[Name not updated]" : product.Name)}");
            Console.WriteLine($"    - Quantity on hand: {product.QuantityOnHand:N0}");
            Console.WriteLine($"    - Unit value: {unitValue:C}");
            Console.WriteLine($"    - Inventory value: {product.TotalStockValue:C}");
            Console.WriteLine($"    - Inventory age: {age:F2} days");

            rank++;
        }
    }
}
