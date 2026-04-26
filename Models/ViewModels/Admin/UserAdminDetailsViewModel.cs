namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class UserAdminDetailsViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string CurrentRole { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; }

    public bool IsLockedOut { get; set; }

    public bool IsCurrentUser { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int TotalOrders { get; set; }

    public int CompletedOrders { get; set; }

    public decimal TotalSpent { get; set; }

    public DateTime? LastOrderAtUtc { get; set; }

    public UserAdminUpdateViewModel UpdateForm { get; set; } = new();
}