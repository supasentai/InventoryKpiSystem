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
            CancellationToken cancellationToken) =>
        {
            var inventory = await TryGetDatabaseInventory(inventoryRepository, cancellationToken);
            var snapshot = kpiService.CreateSnapshot(
                inventory.Count > 0
                    ? inventory.ToList()
                    : inventoryService.GetAllInventory());

            return Results.Ok(snapshot);
        })
        .WithName("GetKpis")
        .WithTags("KPIs")
        .WithSummary("Calculates KPI values from the current inventory state.")
        .Produces<KpiResult>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IReadOnlyList<InventoryItem>> TryGetDatabaseInventory(
        IInventoryRepository inventoryRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            return await inventoryRepository.GetAllAsync(cancellationToken);
        }
        catch
        {
            return Array.Empty<InventoryItem>();
        }
    }
}
