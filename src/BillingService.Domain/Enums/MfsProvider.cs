namespace BillingService.Domain.Enums;

// Only meaningful when PaymentMethod is Mfs. Extend this list as you support more providers.
public enum MfsProvider
{
    None = 0,
    Bkash = 1,
    Nagad = 2,
    Rocket = 3,
    Upay = 4
}
