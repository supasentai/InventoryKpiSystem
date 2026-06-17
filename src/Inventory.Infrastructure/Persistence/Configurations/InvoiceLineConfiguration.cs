using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");

        builder.Property<int>("Id")
            .ValueGeneratedOnAdd();

        builder.HasKey("Id");

        builder.Property<string>("InvoiceID")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(line => line.ItemCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(line => line.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(line => line.UnitAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.HasIndex(line => line.ItemCode);
    }
}
