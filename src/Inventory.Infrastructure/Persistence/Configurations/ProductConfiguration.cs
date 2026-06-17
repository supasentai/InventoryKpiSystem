using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.ProductId);

        builder.Property(product => product.ProductId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(product => product.ItemCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(product => product.ItemCode)
            .IsUnique();
    }
}
