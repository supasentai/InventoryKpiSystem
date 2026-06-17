using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(invoice => invoice.InvoiceID);

        builder.Property(invoice => invoice.InvoiceID)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(invoice => invoice.InvoiceNumber)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(invoice => invoice.Type)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(invoice => invoice.Date)
            .IsRequired();

        builder.HasMany(invoice => invoice.LineItems)
            .WithOne()
            .HasForeignKey("InvoiceID")
            .HasPrincipalKey(invoice => invoice.InvoiceID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(invoice => invoice.InvoiceNumber);
    }
}
