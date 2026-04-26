using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Models.ViewModels.Order;

public class OrderDetailsViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string PaymentProvider { get; set; } = string.Empty;

    public string? PaymentTransactionReference { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public string ShippingFullName { get; set; } = string.Empty;

    public string ShippingEmail { get; set; } = string.Empty;

    public string ShippingPhone { get; set; } = string.Empty;

    public string ShippingAddress { get; set; } = string.Empty;

    public string ShippingWard { get; set; } = string.Empty;

    public string ShippingProvince { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string? ShipmentCarrier { get; set; }

    public string? TrackingNumber { get; set; }

    public string? TrackingUrl { get; set; }

    public DateTime? EstimatedDeliveryDateUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public string? PromoCode { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string CurrencyCode { get; set; } = "VND";

    public DateTime? CancelledAtUtc { get; set; }

    public string? CancellationReason { get; set; }

    public string? CancelledByName { get; set; }

    public bool IsRefundReady { get; set; }

    public DateTime? RefundReadyAtUtc { get; set; }

    public bool CanBeCancelled { get; set; }

    public bool IsGuestOrder { get; set; }

    public IReadOnlyList<OrderStatusHistoryItemViewModel> StatusHistory { get; set; } = [];

    public IReadOnlyList<OrderConfirmationItemViewModel> Items { get; set; } = [];

    public int ItemCount { get; set; }

    public decimal Subtotal { get; set; }
}