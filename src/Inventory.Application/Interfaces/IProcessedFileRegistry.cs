namespace Inventory.Application.Interfaces;

public interface IProcessedFileRegistry
{
    bool IsFileProcessed(string fileName);

    bool MarkAsProcessed(string fileName);
}
