using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _dbContext;

    public ProductRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.ItemCode)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.ExecuteDeleteAsync(cancellationToken);

        var uniqueProducts = products
            .Where(product => !string.IsNullOrWhiteSpace(product.ItemCode))
            .GroupBy(product => product.ItemCode)
            .Select(group => NormalizeProduct(group.Last()))
            .ToList();

        await _dbContext.Products.AddRangeAsync(uniqueProducts, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Product NormalizeProduct(Product product)
    {
        return new Product
        {
            ProductId = string.IsNullOrWhiteSpace(product.ProductId)
                ? product.ItemCode
                : product.ProductId,
            ItemCode = product.ItemCode,
            Name = product.Name
        };
    }
}
