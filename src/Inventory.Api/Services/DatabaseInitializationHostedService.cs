using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Services;

public sealed class DatabaseInitializationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializationHostedService> _logger;

    public DatabaseInitializationHostedService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<InventoryDatabaseSeeder>();

        _logger.LogInformation("Applying database migrations.");
        await dbContext.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Checking whether demo database seed data is required.");
        var seeded = await seeder.SeedAsync(cancellationToken);

        if (seeded)
        {
            _logger.LogInformation("Demo database seed data was inserted.");
            return;
        }

        _logger.LogInformation("Demo database seed data was skipped because existing data was found.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
