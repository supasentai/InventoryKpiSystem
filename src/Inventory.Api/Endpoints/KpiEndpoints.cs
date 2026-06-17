using Inventory.Api.Responses;
using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Api.Endpoints;

internal static class KpiEndpoints
{
    public static IEndpointRouteBuilder MapKpiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/kpis", async (
            IInventoryRepository inventoryRepository,
            IInventoryService inventoryService,
            IKpiService kpiService,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var inventory = await TryGetDatabaseInventory(
                inventoryRepository,
                loggerFactory,
                cancellationToken);

            var snapshot = kpiService.CreateSnapshot(
                inventory.Count > 0
                    ? inventory.ToList()
                    : inventoryService.GetAllInventory());

            return Results.Ok(ApiResponse<KpiResult>.Ok(snapshot));
        })
        .WithName("GetKpis")
        .WithTags("KPIs")
        .WithSummary("Calculates KPI values from the current inventory state.")
        .Produces<ApiResponse<KpiResult>>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IReadOnlyList<InventoryItem>> TryGetDatabaseInventory(
        IInventoryRepository inventoryRepository,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("Inventory.Api.Endpoints.KpiEndpoints");

        try
        {
            return await inventoryRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database inventory read failed. Calculating KPIs from in-memory inventory state.");
            return Array.Empty<InventoryItem>();
        }
    }
}
