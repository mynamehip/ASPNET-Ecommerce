using System.ComponentModel.DataAnnotations;
using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class OrderStatusUpdateViewModel
{
    [Range(1, int.MaxValue)]
    public int OrderId { get; set; }

    [EnumDataType(typeof(OrderStatus))]
    [Display(Name = "Order status")]
    public OrderStatus Status { get; set; }

    [StringLength(120)]
    [Display(Name = "Shipment carrier")]
    public string? ShipmentCarrier { get; set; }

    [StringLength(80)]
    [Display(Name = "Tracking number")]
    public string? TrackingNumber { get; set; }

    [StringLength(260)]
    [Url]
    [Display(Name = "Tracking URL")]
    public string? TrackingUrl { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Estimated delivery date")]
    public DateTime? EstimatedDeliveryDateUtc { get; set; }

    [StringLength(500)]
    [Display(Name = "Workflow note")]
    public string? StatusNote { get; set; }

    [StringLength(500)]
    [Display(Name = "Cancellation reason")]
    public string? CancellationReason { get; set; }
}