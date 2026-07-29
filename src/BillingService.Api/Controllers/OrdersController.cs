using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using BillingService.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IInvoiceService _invoiceService;

    public OrdersController(IOrderService orderService, IInvoiceService invoiceService)
    {
        _orderService = orderService;
        _invoiceService = invoiceService;
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Cashier}")]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CreateOrderAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken ct)
    {
        var order = await _orderService.GetOrderAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    // A fresh order starts as Pending; confirm it once the items are final,
    // then a payment can be recorded (see PaymentsController), which auto-completes it.
    [HttpPost("{id:int}/confirm")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Cashier}")]
    public async Task<ActionResult<OrderResponse>> Confirm(int id, CancellationToken ct)
    {
        var order = await _orderService.ConfirmOrderAsync(id, ct);
        return Ok(order);
    }

    // Only works once a payment has been recorded - generates the Invoice
    // record on first call, then just re-renders the PDF on repeat calls.
    [HttpGet("{id:int}/invoice")]
    public async Task<IActionResult> GetInvoice(int id, CancellationToken ct)
    {
        var (pdfBytes, fileName) = await _invoiceService.GetInvoicePdfAsync(id, ct);
        return File(pdfBytes, "application/pdf", fileName);
    }
}
