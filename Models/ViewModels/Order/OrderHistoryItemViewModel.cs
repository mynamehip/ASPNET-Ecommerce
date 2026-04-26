using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Models.ViewModels.Order;

public class OrderHistoryItemViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string? TrackingNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int ItemCount { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsRefundReady { get; set; }
}