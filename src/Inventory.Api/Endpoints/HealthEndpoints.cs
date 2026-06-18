using Inventory.Api.Responses;
using Inventory.Api.Services;

namespace Inventory.Api.Endpoints;

internal static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(ApiResponse<object>.Ok(new
        {
            status = "Healthy",
            checkedAt = DateTime.UtcNow
        })))
        .WithName("Health")
        .WithTags("Health")
        .WithSummary("Checks whether the API is running.")
        .Produces(StatusCodes.Status200OK);

        app.MapGet("/health/db", async (
            IDatabaseHealthChecker databaseHealthChecker,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Inventory.Api.Endpoints.HealthEndpoints");

            try
            {
                logger.LogInformation("Running database health check.");

                var canConnect = await databaseHealthChecker.CanConnectAsync(cancellationToken);

                logger.LogInformation(
                    "Database health check completed. CanConnect: {CanConnect}",
                    canConnect);

                return canConnect
                    ? Results.Ok(ApiResponse<object>.Ok(new
                    {
                        status = "Healthy",
                        database = "PostgreSQL",
                        checkedAt = DateTime.UtcNow
                    }))
                    : Results.Problem(
                        title: "Database connection failed.",
                        detail: "The API could not connect to the configured PostgreSQL database.",
                        statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database health check failed.");

                return Results.Problem(
                    title: "Database health check failed.",
                    detail: "An unexpected error occurred while checking PostgreSQL connectivity.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        })
        .WithName("DatabaseHealth")
        .WithTags("Health")
        .WithSummary("Checks whether the API can connect to the configured PostgreSQL database.")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
