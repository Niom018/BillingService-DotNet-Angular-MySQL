using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // Stored as strings (e.g. "Mfs", "Bkash") rather than ints so the
        // payment method is human-readable straight from the database.
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.MfsProvider).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(p => p.AmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TransactionReference).HasMaxLength(100);

        builder.HasIndex(p => p.OrderId).IsUnique();
    }
}
