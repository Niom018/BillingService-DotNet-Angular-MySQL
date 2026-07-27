namespace BillingService.Application.DTOs;

public record CreateOrderItemRequest(int ProductId, int Quantity);

public record CreateOrderRequest(int CustomerId, List<CreateOrderItemRequest> Items);

public record RecordPaymentRequest(
    string Method,              // "Cash" | "Card" | "Mfs"
    string? MfsProvider,        // "Bkash" | "Nagad" | "Rocket" | "Upay" (required when Method is "Mfs")
    string? TransactionReference,
    decimal AmountPaid);

public record OrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record OrderResponse(
    int Id,
    string OrderNumber,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    List<OrderItemResponse> Items,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string? PaymentMethod,
    string? PaymentStatus);
