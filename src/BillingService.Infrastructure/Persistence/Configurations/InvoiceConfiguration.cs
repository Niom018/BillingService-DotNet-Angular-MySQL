using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.Property(i => i.InvoiceNumber).HasMaxLength(30).IsRequired();
        builder.Property(i => i.PdfPath).HasMaxLength(500);
        builder.HasIndex(i => i.OrderId).IsUnique();
    }
}
