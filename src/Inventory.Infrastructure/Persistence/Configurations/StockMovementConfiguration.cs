using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.Property<int>("Id")
            .ValueGeneratedOnAdd();

        builder.HasKey("Id");

        builder.Property(movement => movement.ItemCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(movement => movement.Type)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(movement => movement.Quantity)
            .IsRequired();

        builder.Property(movement => movement.UnitCost)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(movement => movement.MovementDate)
            .IsRequired();

        builder.HasIndex(movement => movement.ItemCode);
    }
}
