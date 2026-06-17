using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.InventoryItems
            .AsNoTracking()
            .Include(item => item.PurchaseBatches)
            .OrderBy(item => item.ItemCode)
            .ToListAsync(cancellationToken);

        var saleMovements = await _dbContext.StockMovements
            .AsNoTracking()
            .Where(movement => movement.Type == StockMovementType.Sale)
            .OrderBy(movement => movement.MovementDate)
            .ToListAsync(cancellationToken);

        var saleDateLookup = saleMovements
            .GroupBy(movement => movement.ItemCode)
            .ToDictionary(
                group => group.Key,
                group => group.Select(movement => movement.MovementDate).ToList());

        foreach (var item in items)
        {
            if (saleDateLookup.TryGetValue(item.ItemCode, out var dates))
            {
                item.SaleDates = dates;
            }
        }

        return items;
    }

    public async Task ReplaceAsync(
        IEnumerable<InventoryItem> inventoryItems,
        IEnumerable<StockMovement> stockMovements,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.StockLots.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.StockMovements.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.InventoryItems.ExecuteDeleteAsync(cancellationToken);

        var items = inventoryItems
            .Where(item => !string.IsNullOrWhiteSpace(item.ItemCode))
            .GroupBy(item => item.ItemCode)
            .Select(group => NormalizeInventoryItem(group.Last()))
            .ToList();

        var movements = stockMovements
            .Where(movement => !string.IsNullOrWhiteSpace(movement.ItemCode))
            .Select(NormalizeStockMovement)
            .ToList();

        await _dbContext.InventoryItems.AddRangeAsync(items, cancellationToken);
        await _dbContext.StockMovements.AddRangeAsync(movements, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static InventoryItem NormalizeInventoryItem(InventoryItem item)
    {
        return new InventoryItem
        {
            ProductId = string.IsNullOrWhiteSpace(item.ProductId)
                ? item.ItemCode
                : item.ProductId,
            ItemCode = item.ItemCode,
            Name = item.Name,
            TotalSoldQuantity = item.TotalSoldQuantity,
            PurchaseBatches = item.PurchaseBatches
                .Select(lot => new StockLot
                {
                    PurchaseDate = AsUtc(lot.PurchaseDate),
                    UnitCost = lot.UnitCost,
                    InitialQuantity = lot.InitialQuantity,
                    RemainingQuantity = lot.RemainingQuantity
                })
                .ToList()
        };
    }

    private static StockMovement NormalizeStockMovement(StockMovement movement)
    {
        return new StockMovement
        {
            ItemCode = movement.ItemCode,
            Type = movement.Type,
            Quantity = movement.Quantity,
            UnitCost = movement.UnitCost,
            MovementDate = AsUtc(movement.MovementDate)
        };
    }

    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
