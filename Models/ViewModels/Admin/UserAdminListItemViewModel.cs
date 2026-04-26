namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class UserAdminListItemViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; }

    public bool IsLockedOut { get; set; }

    public int TotalOrders { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}