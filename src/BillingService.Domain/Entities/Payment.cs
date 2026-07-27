using BillingService.Domain.Common;
using BillingService.Domain.Enums;

namespace BillingService.Domain.Entities;

public class Payment : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public PaymentMethod Method { get; private set; }
    public MfsProvider MfsProvider { get; private set; } = MfsProvider.None;
    public string? TransactionReference { get; private set; }

    public decimal AmountPaid { get; private set; }
    public DateTime PaymentDate { get; private set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    private Payment() { }

    public static Payment CreateCashPayment(Order order, decimal amount)
    {
        return new Payment
        {
            OrderId = order.Id,
            Order = order,
            Method = PaymentMethod.Cash,
            AmountPaid = amount,
            Status = PaymentStatus.Completed
        };
    }

    public static Payment CreateCardPayment(Order order, decimal amount, string transactionReference)
    {
        return new Payment
        {
            OrderId = order.Id,
            Order = order,
            Method = PaymentMethod.Card,
            AmountPaid = amount,
            TransactionReference = transactionReference,
            Status = PaymentStatus.Completed
        };
    }

    public static Payment CreateMfsPayment(Order order, decimal amount, MfsProvider provider, string transactionReference)
    {
        if (provider == MfsProvider.None)
            throw new ArgumentException("A specific MFS provider is required.", nameof(provider));

        return new Payment
        {
            OrderId = order.Id,
            Order = order,
            Method = PaymentMethod.Mfs,
            MfsProvider = provider,
            AmountPaid = amount,
            TransactionReference = transactionReference,
            Status = PaymentStatus.Completed
        };
    }

    public void MarkAsFailed() => Status = PaymentStatus.Failed;
    public void MarkAsRefunded() => Status = PaymentStatus.Refunded;
}
