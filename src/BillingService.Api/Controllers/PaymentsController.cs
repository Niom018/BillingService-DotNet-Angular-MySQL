using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using BillingService.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/orders/{orderId:int}/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Cashier}")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    // Method is "Cash" | "Card" | "Mfs". For Mfs, also send MfsProvider
    // ("Bkash" | "Nagad" | "Rocket" | "Upay") and TransactionReference.
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> RecordPayment(
        int orderId, RecordPaymentRequest request, CancellationToken ct)
    {
        var order = await _paymentService.RecordPaymentAsync(orderId, request, ct);
        return Ok(order);
    }
}
