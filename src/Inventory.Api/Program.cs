using Inventory.Api.Endpoints;
using Inventory.Api.Extensions;
using Inventory.Api.Middleware;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine("logs", "inventory-api-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Inventory API.");

    var builder = WebApplication.CreateBuilder(args);
    builder.AddInventoryLogging();

    builder.Services.AddProblemDetails();
    builder.Services.AddInventoryApiDocumentation();
    builder.Services.AddInventoryApplication();
    builder.Services.AddInventoryInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

    var app = builder.Build();

    app.Lifetime.ApplicationStarted.Register(() => Log.Information("Inventory API started."));
    app.Lifetime.ApplicationStopping.Register(() => Log.Information("Inventory API shutting down."));

    app.UseInventoryCorrelationId();
    app.UseInventoryExceptionHandling();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var correlationId = httpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var value)
                ? value?.ToString()
                : httpContext.TraceIdentifier;

            diagnosticContext.Set("CorrelationId", correlationId);
            diagnosticContext.Set("Route", httpContext.GetEndpoint()?.DisplayName ?? httpContext.Request.Path.Value);
        };
    });

    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();

    app.MapHealthEndpoints();
    app.MapProductEndpoints();
    app.MapInventoryEndpoints();
    app.MapKpiEndpoints();
    app.MapImportEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Inventory API terminated unexpectedly.");
    throw;
}
finally
{
    Log.Information("Inventory API stopped.");
    Log.CloseAndFlush();
}

public partial class Program;
