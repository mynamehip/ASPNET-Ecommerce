using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.ViewModels.Order;

public class OrderLookupViewModel
{
    [Required]
    [StringLength(32)]
    [Display(Name = "Order number")]
    public string OrderNumber { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string? Email { get; set; }
}