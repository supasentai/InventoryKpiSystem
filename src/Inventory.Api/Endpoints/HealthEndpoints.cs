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

        return app;
    }
}
