using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class OrderAdminListItemViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int ItemCount { get; set; }

    public decimal Subtotal { get; set; }
}