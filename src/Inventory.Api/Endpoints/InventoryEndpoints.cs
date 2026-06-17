using Inventory.Application.Interfaces;

namespace Inventory.Api.Endpoints;

internal static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inventory", async (
            IInventoryRepository inventoryRepository,
            IInventoryService inventoryService,
            CancellationToken cancellationToken) =>
        {
            var databaseInventory = await TryGetDatabaseInventory(inventoryRepository, cancellationToken);
            if (databaseInventory.Count > 0)
            {
                return Results.Ok(databaseInventory);
            }

            var inventory = inventoryService.GetAllInventory()
                .OrderBy(item => item.ItemCode)
                .Select(item => new
                {
                    item.ProductId,
                    item.ItemCode,
                    item.Name,
                    item.QuantityOnHand,
                    item.TotalSoldQuantity,
                    item.TotalStockValue,
                    PurchaseBatches = item.PurchaseBatches
                        .OrderBy(batch => batch.PurchaseDate)
                        .Select(batch => new
                        {
                            batch.PurchaseDate,
                            batch.UnitCost,
                            batch.InitialQuantity,
                            batch.RemainingQuantity
                        })
                });

            return Results.Ok(inventory);
        })
        .WithName("GetInventory")
        .WithTags("Inventory")
        .WithSummary("Lists inventory quantities, value, sales, and FIFO purchase lots.")
        .Produces(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IReadOnlyList<object>> TryGetDatabaseInventory(
        IInventoryRepository inventoryRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var inventory = await inventoryRepository.GetAllAsync(cancellationToken);
            return inventory
                .Select(item => new
                {
                    item.ProductId,
                    item.ItemCode,
                    item.Name,
                    item.QuantityOnHand,
                    item.TotalSoldQuantity,
                    item.TotalStockValue,
                    PurchaseBatches = item.PurchaseBatches
                        .OrderBy(batch => batch.PurchaseDate)
                        .Select(batch => new
                        {
                            batch.PurchaseDate,
                            batch.UnitCost,
                            batch.InitialQuantity,
                            batch.RemainingQuantity
                        })
                })
                .Cast<object>()
                .ToList();
        }
        catch
        {
            return Array.Empty<object>();
        }
    }
}
