using BillingService.Application.Exceptions;
using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;

namespace BillingService.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvoicePdfGenerator _pdfGenerator;

    public InvoiceService(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IInvoicePdfGenerator pdfGenerator)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<(byte[] PdfBytes, string FileName)> GetInvoicePdfAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
            ?? throw NotFoundException.For<Order>(orderId);

        if (order.Payment is null)
            throw new InvalidOperationException("An invoice can only be generated after payment has been recorded.");

        if (order.Invoice is null)
        {
            // Deterministic from the order number, so no separate uniqueness check is needed.
            var invoiceNumber = order.OrderNumber.Replace("ORD-", "INV-");
            order.Invoice = new Invoice(order, invoiceNumber);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var invoice = order.Invoice ?? throw new InvalidOperationException("Invoice unexpectedly missing after creation.");
        var pdfBytes = _pdfGenerator.Generate(order, invoice);
        return (pdfBytes, $"{invoice.InvoiceNumber}.pdf");
    }
}
