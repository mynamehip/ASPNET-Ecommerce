using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class AdminDashboardRecentOrderViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public decimal Subtotal { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}