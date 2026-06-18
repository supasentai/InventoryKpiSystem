using System.Net;
using System.Text.Json;
using FluentAssertions;
using Inventory.Api.Services;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Inventory.Application.Tests;

public class ApiEndpointTests : IClassFixture<ApiEndpointTests.InventoryApiFactory>
{
    private readonly HttpClient _client;

    public ApiEndpointTests(InventoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/db")]
    [InlineData("/api/products")]
    [InlineData("/api/inventory")]
    [InlineData("/api/kpis")]
    public async Task GetEndpoints_ShouldReturnSuccessfulApiResponse(string url)
    {
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        document.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        document.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    public sealed class InventoryApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDatabaseHealthChecker>();
                services.RemoveAll<IProductRepository>();
                services.RemoveAll<IInventoryRepository>();
                services.RemoveAll<IInvoiceRepository>();

                services.AddSingleton<IDatabaseHealthChecker, HealthyDatabaseHealthChecker>();
                services.AddSingleton<IProductRepository, TestProductRepository>();
                services.AddSingleton<IInventoryRepository, TestInventoryRepository>();
                services.AddSingleton<IInvoiceRepository, TestInvoiceRepository>();
            });
        }
    }

    private sealed class HealthyDatabaseHealthChecker : IDatabaseHealthChecker
    {
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class TestProductRepository : IProductRepository
    {
        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Product> products =
            [
                new Product
                {
                    ProductId = "P-1",
                    ItemCode = "SKU-1",
                    Name = "Test Product"
                }
            ];

            return Task.FromResult(products);
        }

        public Task ReplaceAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestInventoryRepository : IInventoryRepository
    {
        public Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<InventoryItem> items =
            [
                new InventoryItem
                {
                    ProductId = "P-1",
                    ItemCode = "SKU-1",
                    Name = "Test Product",
                    TotalSoldQuantity = 2,
                    SaleDates = [new DateTime(2024, 1, 2)],
                    PurchaseBatches =
                    [
                        new StockLot
                        {
                            PurchaseDate = new DateTime(2024, 1, 1),
                            UnitCost = 10m,
                            InitialQuantity = 5,
                            RemainingQuantity = 3
                        }
                    ]
                }
            ];

            return Task.FromResult(items);
        }

        public Task ReplaceAsync(
            IEnumerable<InventoryItem> inventoryItems,
            IEnumerable<StockMovement> stockMovements,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestInvoiceRepository : IInvoiceRepository
    {
        public Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Invoice>>(Array.Empty<Invoice>());
        }

        public Task ReplaceAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
