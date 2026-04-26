using System.ComponentModel.DataAnnotations;
using ASPNET_Ecommerce.Models.Orders;
using ASPNET_Ecommerce.Models.ViewModels.Cart;

namespace ASPNET_Ecommerce.Models.ViewModels.Order;

public class CheckoutViewModel
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(32)]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Ward")]
    public string Ward { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Province")]
    public string Province { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Delivery notes")]
    public string? Notes { get; set; }

    [StringLength(40)]
    [Display(Name = "Promo code")]
    public string? PromoCode { get; set; }

    [Display(Name = "Save as default checkout address")]
    public bool SaveAsDefaultAddress { get; set; }

    [EnumDataType(typeof(PaymentMethod))]
    [Display(Name = "Payment method")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];

    public int ItemCount { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal TaxRatePercentage { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? AppliedPromoDiscountPercentage { get; set; }

    public string? AppliedPromoCode { get; set; }

    public string CurrencyCode { get; set; } = "VND";

    public decimal? FreeShippingThreshold { get; set; }

    public string? PaymentInstructions { get; set; }

    public bool HasItems => Items.Count > 0;
}