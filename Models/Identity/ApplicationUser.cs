using Microsoft.AspNetCore.Identity;

namespace ASPNET_Ecommerce.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? DefaultAddress { get; set; }

    public string? DefaultWard { get; set; }

    public string? DefaultProvince { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}