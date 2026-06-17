using Inventory.Infrastructure.Persistence;

namespace Inventory.Api.Services;

public class DatabaseHealthChecker : IDatabaseHealthChecker
{
    private readonly InventoryDbContext _dbContext;

    public DatabaseHealthChecker(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.CanConnectAsync(cancellationToken);
    }
}
