using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Seed;

public sealed class InventoryDatabaseSeeder
{
    private readonly InventoryDbContext _dbContext;

    public InventoryDatabaseSeeder(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await HasExistingDataAsync(cancellationToken))
        {
            return false;
        }

        var seedData = DemoInventorySeedData.Create();

        await _dbContext.Products.AddRangeAsync(seedData.Products, cancellationToken);
        await _dbContext.InventoryItems.AddRangeAsync(seedData.InventoryItems, cancellationToken);
        await _dbContext.StockMovements.AddRangeAsync(seedData.StockMovements, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<bool> HasExistingDataAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Products.AnyAsync(cancellationToken)
            || await _dbContext.InventoryItems.AnyAsync(cancellationToken)
            || await _dbContext.StockLots.AnyAsync(cancellationToken)
            || await _dbContext.StockMovements.AnyAsync(cancellationToken);
    }
}

internal static class DemoInventorySeedData
{
    public static InventorySeedData Create()
    {
        var products = new List<Product>
        {
            new()
            {
                ProductId = "DEMO-CHAIR",
                ItemCode = "CHAIR-001",
                Name = "Ergonomic Office Chair"
            },
            new()
            {
                ProductId = "DEMO-DESK",
                ItemCode = "DESK-001",
                Name = "Standing Desk"
            },
            new()
            {
                ProductId = "DEMO-MONITOR",
                ItemCode = "MON-001",
                Name = "27 Inch Monitor"
            },
            new()
            {
                ProductId = "DEMO-KEYBOARD",
                ItemCode = "KEY-001",
                Name = "Mechanical Keyboard"
            }
        };

        var inventoryItems = new List<InventoryItem>
        {
            new()
            {
                ProductId = "DEMO-CHAIR",
                ItemCode = "CHAIR-001",
                Name = "Ergonomic Office Chair",
                TotalSoldQuantity = 8,
                PurchaseBatches =
                [
                    Lot(2026, 1, 5, 125m, 20, 7),
                    Lot(2026, 2, 14, 130m, 10, 5)
                ]
            },
            new()
            {
                ProductId = "DEMO-DESK",
                ItemCode = "DESK-001",
                Name = "Standing Desk",
                TotalSoldQuantity = 3,
                PurchaseBatches =
                [
                    Lot(2026, 1, 8, 320m, 8, 5)
                ]
            },
            new()
            {
                ProductId = "DEMO-MONITOR",
                ItemCode = "MON-001",
                Name = "27 Inch Monitor",
                TotalSoldQuantity = 6,
                PurchaseBatches =
                [
                    Lot(2026, 1, 12, 210m, 12, 4),
                    Lot(2026, 2, 12, 205m, 8, 6)
                ]
            },
            new()
            {
                ProductId = "DEMO-KEYBOARD",
                ItemCode = "KEY-001",
                Name = "Mechanical Keyboard",
                TotalSoldQuantity = 15,
                PurchaseBatches =
                [
                    Lot(2026, 1, 20, 45m, 30, 15)
                ]
            }
        };

        var stockMovements = new List<StockMovement>
        {
            Purchase("CHAIR-001", 20, 125m, 2026, 1, 5),
            Sale("CHAIR-001", 8, 125m, 2026, 2, 1),
            Purchase("CHAIR-001", 10, 130m, 2026, 2, 14),
            Purchase("DESK-001", 8, 320m, 2026, 1, 8),
            Sale("DESK-001", 3, 320m, 2026, 2, 3),
            Purchase("MON-001", 12, 210m, 2026, 1, 12),
            Sale("MON-001", 6, 210m, 2026, 2, 10),
            Purchase("MON-001", 8, 205m, 2026, 2, 12),
            Purchase("KEY-001", 30, 45m, 2026, 1, 20),
            Sale("KEY-001", 15, 45m, 2026, 2, 18)
        };

        return new InventorySeedData(products, inventoryItems, stockMovements);
    }

    private static StockLot Lot(
        int year,
        int month,
        int day,
        decimal unitCost,
        int initialQuantity,
        int remainingQuantity)
    {
        return new StockLot
        {
            PurchaseDate = UtcDate(year, month, day),
            UnitCost = unitCost,
            InitialQuantity = initialQuantity,
            RemainingQuantity = remainingQuantity
        };
    }

    private static StockMovement Purchase(
        string itemCode,
        int quantity,
        decimal unitCost,
        int year,
        int month,
        int day)
    {
        return Movement(itemCode, StockMovementType.Purchase, quantity, unitCost, year, month, day);
    }

    private static StockMovement Sale(
        string itemCode,
        int quantity,
        decimal unitCost,
        int year,
        int month,
        int day)
    {
        return Movement(itemCode, StockMovementType.Sale, quantity, unitCost, year, month, day);
    }

    private static StockMovement Movement(
        string itemCode,
        StockMovementType type,
        int quantity,
        decimal unitCost,
        int year,
        int month,
        int day)
    {
        return new StockMovement
        {
            ItemCode = itemCode,
            Type = type,
            Quantity = quantity,
            UnitCost = unitCost,
            MovementDate = UtcDate(year, month, day)
        };
    }

    private static DateTime UtcDate(int year, int month, int day)
    {
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}

internal sealed record InventorySeedData(
    IReadOnlyList<Product> Products,
    IReadOnlyList<InventoryItem> InventoryItems,
    IReadOnlyList<StockMovement> StockMovements);
