using Microsoft.AspNetCore.Identity;

namespace BillingService.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
}
