using System.ComponentModel.DataAnnotations;
using ASPNET_Ecommerce.Models.Identity;

namespace ASPNET_Ecommerce.Models.Orders;

public class Order
{
    public int Id { get; set; }

    [Required]
    [StringLength(32)]
    public string OrderNumber { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [StringLength(80)]
    public string PaymentProvider { get; set; } = string.Empty;

    [StringLength(80)]
    public string? PaymentTransactionReference { get; set; }

    [StringLength(500)]
    public string? PaymentFailureReason { get; set; }

    [Required]
    [StringLength(120)]
    public string ShippingFullName { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string ShippingEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    [Phone]
    public string ShippingPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string ShippingWard { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string ShippingProvince { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(120)]
    public string? ShipmentCarrier { get; set; }

    [StringLength(80)]
    public string? TrackingNumber { get; set; }

    [StringLength(260)]
    public string? TrackingUrl { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Subtotal { get; set; }

    [StringLength(40)]
    public string? PromoCode { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PromoDiscountPercentage { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal DiscountAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal ShippingFee { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal TaxRate { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal TaxAmount { get; set; }

    [Required]
    [StringLength(12)]
    public string CurrencyCode { get; set; } = "VND";

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal TotalAmount { get; set; }

    public bool IsInventoryRestored { get; set; }

    public bool IsRefundReady { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PaymentAuthorizedAtUtc { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public DateTime? EstimatedDeliveryDateUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }

    [StringLength(500)]
    public string? CancellationReason { get; set; }

    [StringLength(450)]
    public string? CancelledByUserId { get; set; }

    [StringLength(120)]
    public string? CancelledByName { get; set; }

    public DateTime? RefundReadyAtUtc { get; set; }

    public DateTime? RefundedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];
}