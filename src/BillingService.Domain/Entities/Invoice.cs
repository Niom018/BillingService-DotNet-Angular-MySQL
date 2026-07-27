using BillingService.Domain.Common;
using BillingService.Domain.Enums;

namespace BillingService.Domain.Entities;

public class Invoice : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; } = DateTime.UtcNow;

    // Populated once the PDF service in the Application layer generates the file.
    public string? PdfPath { get; set; }
    public int? GeneratedByUserId { get; set; }

    private Invoice() { }

    public Invoice(Order order, string invoiceNumber, int? generatedByUserId = null)
    {
        if (order.Status is not (OrderStatus.Confirmed or OrderStatus.Completed))
            throw new InvalidOperationException("Invoice can only be generated for confirmed or completed orders.");

        OrderId = order.Id;
        Order = order;
        InvoiceNumber = invoiceNumber;
        GeneratedByUserId = generatedByUserId;
    }
}
