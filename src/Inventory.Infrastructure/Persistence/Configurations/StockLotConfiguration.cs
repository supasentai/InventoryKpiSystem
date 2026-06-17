using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockLotConfiguration : IEntityTypeConfiguration<StockLot>
{
    public void Configure(EntityTypeBuilder<StockLot> builder)
    {
        builder.ToTable("stock_lots");

        builder.Property<int>("Id")
            .ValueGeneratedOnAdd();

        builder.HasKey("Id");

        builder.Property<string>("InventoryItemCode")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(lot => lot.PurchaseDate)
            .IsRequired();

        builder.Property(lot => lot.UnitCost)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(lot => lot.InitialQuantity)
            .IsRequired();

        builder.Property(lot => lot.RemainingQuantity)
            .IsRequired();
    }
}
