namespace BillingService.Application.DTOs;

public record CreateProductRequest(string Sku, string Name, string? Description, decimal UnitPrice, int StockQuantity);

public record ProductResponse(int Id, string Sku, string Name, string? Description, decimal UnitPrice, int StockQuantity);
