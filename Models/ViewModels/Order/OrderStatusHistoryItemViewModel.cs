using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Models.ViewModels.Order;

public class OrderStatusHistoryItemViewModel
{
    public OrderStatus PreviousStatus { get; set; }

    public OrderStatus NewStatus { get; set; }

    public string? Note { get; set; }

    public string ChangedByName { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }
}