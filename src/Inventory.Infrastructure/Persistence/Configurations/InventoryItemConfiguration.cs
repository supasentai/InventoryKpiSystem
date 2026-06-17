using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");

        builder.HasKey(item => item.ItemCode);

        builder.Property(item => item.ItemCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.ProductId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(item => item.TotalSoldQuantity)
            .IsRequired();

        builder.Ignore(item => item.QuantityOnHand);
        builder.Ignore(item => item.TotalStockValue);
        builder.Ignore(item => item.SaleDates);

        builder.HasMany(item => item.PurchaseBatches)
            .WithOne()
            .HasForeignKey("InventoryItemCode")
            .HasPrincipalKey(item => item.ItemCode)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
