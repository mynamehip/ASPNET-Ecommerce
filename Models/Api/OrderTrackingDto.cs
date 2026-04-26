namespace ASPNET_Ecommerce.Models.Api;

public class OrderTrackingDto
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string PaymentProvider { get; set; } = string.Empty;

    public string? PaymentTransactionReference { get; set; }

    public string ShippingFullName { get; set; } = string.Empty;

    public string ShippingWard { get; set; } = string.Empty;

    public string ShippingProvince { get; set; } = string.Empty;

    public string? ShipmentCarrier { get; set; }

    public string? TrackingNumber { get; set; }

    public string? TrackingUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? EstimatedDeliveryDateUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }
}