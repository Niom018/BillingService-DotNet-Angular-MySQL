namespace BillingService.Application.DTOs;

public record RegisterRequest(string FullName, string Email, string Password);

public record LoginRequest(string Email, string Password);

// Admin-only endpoint for creating Manager/Cashier accounts with a chosen role.
public record CreateStaffRequest(string FullName, string Email, string Password, string Role);

public record AuthResponse(string Token, string Email, string FullName, string[] Roles);
