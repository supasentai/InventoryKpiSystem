using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IFifoCostingService
{
    void ApplySale(InventoryItem item, int quantity);
}
