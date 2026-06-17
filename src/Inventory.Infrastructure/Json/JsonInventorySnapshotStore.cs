using System.Text.Json;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Infrastructure.Json;

public class JsonInventorySnapshotStore : IInventorySnapshotStore
{
    private readonly string _snapshotFilePath;
    private readonly object _fileLock = new();

    public JsonInventorySnapshotStore(string snapshotFilePath = "inventory-snapshot.json")
    {
        _snapshotFilePath = snapshotFilePath;
    }

    public IReadOnlyDictionary<string, InventoryItem> Load()
    {
        if (!File.Exists(_snapshotFilePath))
        {
            return new Dictionary<string, InventoryItem>();
        }

        lock (_fileLock)
        {
            try
            {
                var json = File.ReadAllText(_snapshotFilePath);
                var snapshotData = JsonSerializer.Deserialize<Dictionary<string, InventoryItem>>(json);

                if (snapshotData is null)
                {
                    return new Dictionary<string, InventoryItem>();
                }

                Console.WriteLine($"[System] Loaded inventory snapshot with {snapshotData.Count} products.");
                return snapshotData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Snapshot Error] Cannot load inventory state: {ex.Message}");
                return new Dictionary<string, InventoryItem>();
            }
        }
    }

    public void Save(IReadOnlyDictionary<string, InventoryItem> items)
    {
        lock (_fileLock)
        {
            try
            {
                var snapshotData = items.ToDictionary(pair => pair.Key, pair => pair.Value);
                var json = JsonSerializer.Serialize(snapshotData, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(_snapshotFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Snapshot Error] Cannot save inventory state: {ex.Message}");
            }
        }
    }
}
