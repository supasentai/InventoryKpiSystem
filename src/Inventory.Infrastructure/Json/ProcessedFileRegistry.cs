using System.Collections.Concurrent;
using System.Text.Json;
using Inventory.Application.Interfaces;

namespace Inventory.Infrastructure.Json;

public class ProcessedFileRegistry : IProcessedFileRegistry
{
    private readonly string _registryFilePath;
    private readonly ConcurrentDictionary<string, bool> _processedFiles = new();
    private readonly object _fileLock = new();

    public ProcessedFileRegistry(string storageDirectory = "processed-files")
    {
        Directory.CreateDirectory(storageDirectory);
        _registryFilePath = Path.Combine(storageDirectory, "processed-files.json");
        LoadRegistry();
    }

    public bool IsFileProcessed(string fileName)
    {
        return _processedFiles.ContainsKey(fileName);
    }

    public bool MarkAsProcessed(string fileName)
    {
        if (!_processedFiles.TryAdd(fileName, true))
        {
            return false;
        }

        SaveRegistry();
        return true;
    }

    private void LoadRegistry()
    {
        if (!File.Exists(_registryFilePath))
        {
            return;
        }

        lock (_fileLock)
        {
            try
            {
                var json = File.ReadAllText(_registryFilePath);
                var files = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

                foreach (var file in files)
                {
                    _processedFiles.TryAdd(file, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Registry Error] Cannot load processed file history: {ex.Message}");
            }
        }
    }

    private void SaveRegistry()
    {
        lock (_fileLock)
        {
            try
            {
                var files = new List<string>(_processedFiles.Keys);
                var json = JsonSerializer.Serialize(files, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(_registryFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Registry Error] Cannot save processed file history: {ex.Message}");
            }
        }
    }
}
