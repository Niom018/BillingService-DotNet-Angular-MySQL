using BillingService.Domain.Common;
using BillingService.Domain.Enums;

namespace BillingService.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; private set; } = DateTime.UtcNow;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }

    public decimal Subtotal => _items.Sum(i => i.LineTotal);
    public decimal TotalAmount => Subtotal - DiscountAmount + TaxAmount;

    public Payment? Payment { get; set; }
    public Invoice? Invoice { get; set; }

    private Order() { }

    public Order(Customer customer, string orderNumber)
    {
        CustomerId = customer.Id;
        Customer = customer;
        OrderNumber = orderNumber;
    }

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify an order that is no longer pending.");

        _items.Add(new OrderItem(product, quantity));
    }

    public void Confirm()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot confirm an order with no items.");

        Status = OrderStatus.Confirmed;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Order must be confirmed before it can be completed.");

        Status = OrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed order.");

        Status = OrderStatus.Cancelled;
    }
}
