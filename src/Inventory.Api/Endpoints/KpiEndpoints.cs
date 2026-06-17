using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;

namespace Inventory.Api.Endpoints;

internal static class KpiEndpoints
{
    public static IEndpointRouteBuilder MapKpiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/kpis", (IInventoryService inventoryService, IKpiService kpiService) =>
        {
            var snapshot = kpiService.CreateSnapshot(inventoryService.GetAllInventory());
            return Results.Ok(snapshot);
        })
        .WithName("GetKpis")
        .WithTags("KPIs")
        .WithSummary("Calculates KPI values from the current inventory state.")
        .Produces<KpiResult>(StatusCodes.Status200OK);

        return app;
    }
}
