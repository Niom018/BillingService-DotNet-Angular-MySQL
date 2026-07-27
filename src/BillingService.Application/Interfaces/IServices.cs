using BillingService.Application.DTOs;

namespace BillingService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderResponse> ConfirmOrderAsync(int orderId, CancellationToken ct = default);
    Task<OrderResponse?> GetOrderAsync(int orderId, CancellationToken ct = default);
}

public interface IPaymentService
{
    Task<OrderResponse> RecordPaymentAsync(int orderId, RecordPaymentRequest request, CancellationToken ct = default);
}
