using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.Orders;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public int ProductId { get; set; }

    [Required]
    [StringLength(160)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(64)]
    public string? ProductSku { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal UnitPrice { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}