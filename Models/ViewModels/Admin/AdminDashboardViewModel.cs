namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }

    public int TotalCustomers { get; set; }

    public int TotalEmployees { get; set; }

    public int TotalAdmins { get; set; }

    public int TotalProducts { get; set; }

    public int ActiveProducts { get; set; }

    public int TotalOrders { get; set; }

    public int PendingOrders { get; set; }

    public int HiddenReviews { get; set; }

    public decimal TotalRevenue { get; set; }

    public IReadOnlyList<AdminDashboardRecentOrderViewModel> RecentOrders { get; set; } = [];

    public IReadOnlyList<AdminDashboardLowStockProductViewModel> LowStockProducts { get; set; } = [];
}