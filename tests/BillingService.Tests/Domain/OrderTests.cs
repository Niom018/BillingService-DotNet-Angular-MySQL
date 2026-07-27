using BillingService.Domain.Entities;
using BillingService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace BillingService.Tests.Domain;

public class OrderTests
{
    private static Customer MakeCustomer() => new()
    {
        Id = 1,
        Name = "Sazida Islam",
        Phone = "01700000000"
    };

    private static Product MakeProduct(decimal price = 250m) => new()
    {
        Id = 1,
        Sku = "SKU-001",
        Name = "Sample product",
        UnitPrice = price
    };

    [Fact]
    public void AddItem_CalculatesLineTotal_FromQuantityAndUnitPrice()
    {
        var order = new Order(MakeCustomer(), "INV-0001");
        var product = MakeProduct(price: 250m);

        order.AddItem(product, quantity: 3);

        order.Items.Single().LineTotal.Should().Be(750m);
        order.Subtotal.Should().Be(750m);
    }

    [Fact]
    public void TotalAmount_AppliesDiscountAndTax_OnTopOfSubtotal()
    {
        var order = new Order(MakeCustomer(), "INV-0002");
        order.AddItem(MakeProduct(price: 1000m), quantity: 1);
        order.DiscountAmount = 100m;
        order.TaxAmount = 50m;

        order.TotalAmount.Should().Be(950m); // 1000 - 100 + 50
    }

    [Fact]
    public void Confirm_Throws_WhenOrderHasNoItems()
    {
        var order = new Order(MakeCustomer(), "INV-0003");

        var act = order.Confirm;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddItem_Throws_OnceOrderIsNoLongerPending()
    {
        var order = new Order(MakeCustomer(), "INV-0004");
        order.AddItem(MakeProduct(), quantity: 1);
        order.Confirm();

        var act = () => order.AddItem(MakeProduct(), quantity: 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateMfsPayment_RecordsProviderAndTransactionReference()
    {
        var order = new Order(MakeCustomer(), "INV-0005");
        order.AddItem(MakeProduct(price: 500m), quantity: 1);
        order.Confirm();

        var payment = Payment.CreateMfsPayment(order, amount: 500m, MfsProvider.Bkash, transactionReference: "BKS123456");

        payment.Method.Should().Be(PaymentMethod.Mfs);
        payment.MfsProvider.Should().Be(MfsProvider.Bkash);
        payment.TransactionReference.Should().Be("BKS123456");
        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void CreateMfsPayment_Throws_WhenProviderIsNone()
    {
        var order = new Order(MakeCustomer(), "INV-0006");
        order.AddItem(MakeProduct(), quantity: 1);

        var act = () => Payment.CreateMfsPayment(order, 100m, MfsProvider.None, "ref");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Invoice_Throws_WhenOrderIsStillPending()
    {
        var order = new Order(MakeCustomer(), "INV-0007");
        order.AddItem(MakeProduct(), quantity: 1);

        var act = () => new Invoice(order, "INV-0007");

        act.Should().Throw<InvalidOperationException>();
    }
}
