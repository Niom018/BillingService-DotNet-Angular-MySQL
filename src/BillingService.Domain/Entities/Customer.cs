using BillingService.Domain.Common;

namespace BillingService.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
