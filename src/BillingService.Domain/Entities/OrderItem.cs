using BillingService.Domain.Common;

namespace BillingService.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    // Snapshotted at the time of purchase so historical invoices never
    // change even if the product's list price changes later.
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;

    private OrderItem() { }

    public OrderItem(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        ProductId = product.Id;
        Product = product;
        Quantity = quantity;
        UnitPrice = product.UnitPrice;
    }
}
