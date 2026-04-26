using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Models.ViewModels.Order;

public class OrderTrackingViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string PaymentProvider { get; set; } = string.Empty;

    public string? PaymentTransactionReference { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string ShippingFullName { get; set; } = string.Empty;

    public string ShippingWard { get; set; } = string.Empty;

    public string ShippingProvince { get; set; } = string.Empty;

    public string? ShipmentCarrier { get; set; }

    public string? TrackingNumber { get; set; }

    public string? TrackingUrl { get; set; }

    public DateTime? EstimatedDeliveryDateUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public bool IsGuestOrder { get; set; }

    public bool HasTrackingDetails => !string.IsNullOrWhiteSpace(TrackingNumber) || !string.IsNullOrWhiteSpace(ShipmentCarrier) || !string.IsNullOrWhiteSpace(TrackingUrl);
}