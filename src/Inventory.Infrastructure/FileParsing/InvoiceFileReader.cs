using System.Text.Json;
using System.Text.Json.Serialization;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;

namespace Inventory.Infrastructure.FileParsing;

public class InvoiceFileReader : IInvoiceFileReader
{
    private readonly JsonSerializerOptions _options;

    public InvoiceFileReader(JsonSerializerOptions? options = null)
    {
        _options = options ?? JsonDefaults.CreateOptions();
    }

    public async Task<IReadOnlyList<Invoice>> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var response = JsonSerializer.Deserialize<InvoiceResponse>(json, _options);

        return response?.Invoices?
            .Select(MapInvoice)
            .ToList() ?? new List<Invoice>();
    }

    private static Invoice MapInvoice(InvoiceFileItem item)
    {
        return new Invoice
        {
            InvoiceID = item.InvoiceID,
            InvoiceNumber = item.InvoiceNumber,
            Type = ParseInvoiceType(item.Type),
            Date = item.Date,
            LineItems = item.LineItems ?? new List<InvoiceLine>()
        };
    }

    private static InvoiceType ParseInvoiceType(string type)
    {
        return type switch
        {
            "ACCPAY" => InvoiceType.AccountsPayable,
            "ACCREC" => InvoiceType.AccountsReceivable,
            _ => InvoiceType.Unknown
        };
    }

    private sealed class InvoiceResponse
    {
        public List<InvoiceFileItem> Invoices { get; set; } = new();
    }

    private sealed class InvoiceFileItem
    {
        public string InvoiceID { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("DateString")]
        public DateTime Date { get; set; }

        public List<InvoiceLine>? LineItems { get; set; } = new();
    }
}
