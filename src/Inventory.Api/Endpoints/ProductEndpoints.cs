using Inventory.Api.Responses;
using Inventory.Application.Interfaces;

namespace Inventory.Api.Endpoints;

internal static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async (
            IProductRepository productRepository,
            IInventoryService inventoryService,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var databaseProducts = await TryGetDatabaseProducts(
                productRepository,
                loggerFactory,
                cancellationToken);

            if (databaseProducts.Count > 0)
            {
                return Results.Ok(ApiResponse<IReadOnlyList<object>>.Ok(databaseProducts));
            }

            var products = inventoryService.GetAllInventory()
                .OrderBy(item => item.ItemCode)
                .Select(item => new
                {
                    item.ProductId,
                    item.ItemCode,
                    item.Name
                })
                .Cast<object>()
                .ToList();

            return Results.Ok(ApiResponse<IReadOnlyList<object>>.Ok(products));
        })
        .WithName("GetProducts")
        .WithTags("Products")
        .WithSummary("Lists products loaded into the inventory state.")
        .Produces<ApiResponse<IReadOnlyList<object>>>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IReadOnlyList<object>> TryGetDatabaseProducts(
        IProductRepository productRepository,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("Inventory.Api.Endpoints.ProductEndpoints");

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
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database product read failed. Falling back to in-memory inventory state.");
            return Array.Empty<object>();
        }
    }
}
