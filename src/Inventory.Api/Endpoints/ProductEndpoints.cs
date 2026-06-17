using Inventory.Application.Interfaces;

namespace Inventory.Api.Endpoints;

internal static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", (IInventoryService inventoryService) =>
        {
            var products = inventoryService.GetAllInventory()
                .OrderBy(item => item.ItemCode)
                .Select(item => new
                {
                    item.ProductId,
                    item.ItemCode,
                    item.Name
                });

            return Results.Ok(products);
        })
        .WithName("GetProducts")
        .WithTags("Products")
        .WithSummary("Lists products loaded into the inventory state.")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}
