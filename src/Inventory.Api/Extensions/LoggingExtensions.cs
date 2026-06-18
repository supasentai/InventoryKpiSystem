using Inventory.Api.Middleware;
using Serilog;
using Serilog.Events;

namespace Inventory.Api.Extensions;

internal static class LoggingExtensions
{
    public static WebApplicationBuilder AddInventoryLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine("logs", "inventory-api-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14);
        });

        return builder;
    }

    public static IApplicationBuilder UseInventoryExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }

    public static IApplicationBuilder UseInventoryCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
