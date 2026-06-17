using System.Text.Json;
using System.Text.Json.Serialization;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Infrastructure.FileParsing;

public class ProductFileReader : IProductFileReader
{
    private readonly JsonSerializerOptions _options;

    public ProductFileReader(JsonSerializerOptions? options = null)
    {
        _options = options ?? JsonDefaults.CreateOptions();
    }

    public async Task<IReadOnlyList<Product>> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var response = JsonSerializer.Deserialize<ProductResponse>(json, _options);

        return response?.Items?
            .Where(item => !string.IsNullOrWhiteSpace(item.ItemCode))
            .Select(item => new Product
            {
                ProductId = item.ProductId,
                ItemCode = item.ItemCode,
                Name = item.Name
            })
            .ToList() ?? new List<Product>();
    }

    private sealed class ProductResponse
    {
        public List<ProductItem> Items { get; set; } = new();
    }

    private sealed class ProductItem
    {
        [JsonPropertyName("ItemID")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("Code")]
        public string ItemCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
