namespace BillingService.Infrastructure.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";

    public static readonly string[] All = { Admin, Manager, Cashier };
}
