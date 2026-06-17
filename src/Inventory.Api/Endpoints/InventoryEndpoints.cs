using Inventory.Application.Interfaces;

namespace Inventory.Api.Endpoints;

internal static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inventory", (IInventoryService inventoryService) =>
        {
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
}
