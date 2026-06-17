namespace Inventory.Api.Services;

public interface IDatabaseHealthChecker
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
