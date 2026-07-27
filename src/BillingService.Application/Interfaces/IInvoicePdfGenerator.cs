using BillingService.Domain.Entities;

namespace BillingService.Application.Interfaces;

public interface IInvoicePdfGenerator
{
    /// <summary>
    /// Renders an invoice (order + items + payment info) to a PDF and returns the raw bytes.
    /// The Application layer decides where those bytes get saved.
    /// </summary>
    byte[] Generate(Order order, Invoice invoice);
}
