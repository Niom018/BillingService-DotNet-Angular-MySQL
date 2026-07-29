namespace BillingService.Application.DTOs;

public record CreateCustomerRequest(string Name, string Phone, string? Email, string? Address);

public record CustomerResponse(int Id, string Name, string Phone, string? Email, string? Address);
