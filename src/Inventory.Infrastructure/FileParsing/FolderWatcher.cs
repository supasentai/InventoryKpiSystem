using System.Collections.Concurrent;
using Inventory.Application.Interfaces;

namespace Inventory.Infrastructure.FileParsing;

public class FolderWatcher
{
    private readonly string _invoicesFolder;
    private readonly string _productsFolder;
    private readonly FileProcessor _processor;
    private readonly FileProcessingQueue _queue;
    private readonly Action _onFileProcessed;
    private readonly IInventoryService _inventoryService;
    private readonly IInventorySnapshotStore _snapshotStore;
    private readonly ConcurrentDictionary<string, DateTime> _recentFiles = new();
    private readonly List<FileSystemWatcher> _watchers = new();

    public FolderWatcher(
        string invoicesFolder,
        string productsFolder,
        FileProcessor processor,
        FileProcessingQueue queue,
        Action onFileProcessed,
        IInventoryService inventoryService,
        IInventorySnapshotStore snapshotStore)
    {
        _invoicesFolder = invoicesFolder;
        _productsFolder = productsFolder;
        _processor = processor;
        _queue = queue;
        _onFileProcessed = onFileProcessed;
        _inventoryService = inventoryService;
        _snapshotStore = snapshotStore;
    }

    public void StartMonitoring()
    {
        SetupWatcher(_invoicesFolder);
        SetupWatcher(_productsFolder);

        Task.Run(ProcessQueueAsync);

        Console.WriteLine("[System] File watcher is running in the background.");
    }

    private void SetupWatcher(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var watcher = new FileSystemWatcher(folderPath, "*.txt")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        watcher.Created += (_, eventArgs) => OnFileDetected(eventArgs.FullPath);
        watcher.Changed += (_, eventArgs) => OnFileDetected(eventArgs.FullPath);
        watcher.EnableRaisingEvents = true;

        _watchers.Add(watcher);
    }

    private void OnFileDetected(string filePath)
    {
        if (_recentFiles.TryGetValue(filePath, out var lastTime) &&
            (DateTime.Now - lastTime).TotalSeconds < 2)
        {
            return;
        }

        _recentFiles[filePath] = DateTime.Now;

        var currentTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        var fileName = Path.GetFileName(filePath);

        Console.WriteLine();
        Console.WriteLine("=======================================================");
        Console.WriteLine($"[NEW FILE] {fileName}");
        Console.WriteLine($"[TIME]     {currentTime}");
        Console.WriteLine("=======================================================");
        Console.WriteLine("Processing file, please wait...");

        _ = _queue.EnqueueFileAsync(filePath);
    }

    private async Task ProcessQueueAsync()
    {
        await foreach (var filePath in _queue.Reader.ReadAllAsync())
        {
            var fileName = Path.GetFileName(filePath);

            try
            {
                await Task.Delay(1000);

                if (fileName.Contains("product", StringComparison.OrdinalIgnoreCase))
                {
                    await _processor.ProcessProductFileAsync(filePath);
                }
                else
                {
                    await _processor.ProcessInvoiceFileAsync(filePath);
                }

                _snapshotStore.Save(_inventoryService.Items);
                Console.WriteLine($"[DONE] Data from '{fileName}' was loaded and the snapshot was updated.");
                _onFileProcessed?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"[FILE READ ERROR] {fileName}: {ex.Message}");
                _onFileProcessed?.Invoke();
            }
        }
    }
}
