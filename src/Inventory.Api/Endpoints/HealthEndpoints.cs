using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Endpoints;

internal static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            checkedAt = DateTime.UtcNow
        }))
        .WithName("Health")
        .WithTags("Health")
        .WithSummary("Checks whether the API is running.")
        .Produces(StatusCodes.Status200OK);

        app.MapGet("/health/db", async (InventoryDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? Results.Ok(new
                {
                    status = "Healthy",
                    database = "PostgreSQL",
                    checkedAt = DateTime.UtcNow
                })
                : Results.Problem(
                    title: "Database connection failed.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .WithName("DatabaseHealth")
        .WithTags("Health")
        .WithSummary("Checks whether the API can connect to the configured PostgreSQL database.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
