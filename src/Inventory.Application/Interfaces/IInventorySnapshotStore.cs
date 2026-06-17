using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IInventorySnapshotStore
{
    IReadOnlyDictionary<string, InventoryItem> Load();

    void Save(IReadOnlyDictionary<string, InventoryItem> items);
}
