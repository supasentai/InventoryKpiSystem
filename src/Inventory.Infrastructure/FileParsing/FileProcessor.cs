using Inventory.Application.Interfaces;

namespace Inventory.Infrastructure.FileParsing;

public class FileProcessor
{
    private readonly IProductFileReader _productFileReader;
    private readonly IInvoiceFileReader _invoiceFileReader;
    private readonly IImportService _importService;
    private readonly IProcessedFileRegistry _registry;

    public FileProcessor(
        IProductFileReader productFileReader,
        IInvoiceFileReader invoiceFileReader,
        IImportService importService,
        IProcessedFileRegistry registry)
    {
        _productFileReader = productFileReader;
        _invoiceFileReader = invoiceFileReader;
        _importService = importService;
        _registry = registry;
    }

    public async Task ProcessProductFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(filePath);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var products = await _productFileReader.ReadAsync(filePath, cancellationToken);
                _importService.ImportProducts(products);
                _registry.MarkAsProcessed(fileName);
                break;
            }
            catch (IOException)
            {
                await Task.Delay(500, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON Product Error] File {fileName}: {ex.Message}");
                break;
            }
        }
    }

    public async Task ProcessInvoiceFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(filePath);
        if (_registry.IsFileProcessed(fileName))
        {
            return;
        }

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var invoices = await _invoiceFileReader.ReadAsync(filePath, cancellationToken);
                _importService.ImportInvoices(invoices);
                _registry.MarkAsProcessed(fileName);
                break;
            }
            catch (IOException)
            {
                await Task.Delay(500, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON Invoice Error] File {fileName}: {ex.Message}");
                break;
            }
        }
    }
}
