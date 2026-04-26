using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.Orders;

public class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public OrderStatus PreviousStatus { get; set; }

    public OrderStatus NewStatus { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [StringLength(450)]
    public string? ChangedByUserId { get; set; }

    [StringLength(120)]
    public string ChangedByName { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}