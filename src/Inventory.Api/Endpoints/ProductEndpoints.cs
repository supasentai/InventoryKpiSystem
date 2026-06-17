using Inventory.Application.Interfaces;

namespace Inventory.Api.Endpoints;

internal static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async (
            IProductRepository productRepository,
            IInventoryService inventoryService,
            CancellationToken cancellationToken) =>
        {
            var databaseProducts = await TryGetDatabaseProducts(productRepository, cancellationToken);
            if (databaseProducts.Count > 0)
            {
                return Results.Ok(databaseProducts);
            }

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

    private static async Task<IReadOnlyList<object>> TryGetDatabaseProducts(
        IProductRepository productRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var products = await productRepository.GetAllAsync(cancellationToken);
            return products
                .Select(product => new
                {
                    product.ProductId,
                    product.ItemCode,
                    product.Name
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
