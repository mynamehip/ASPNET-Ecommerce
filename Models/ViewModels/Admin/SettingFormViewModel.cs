using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class SettingFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Store name")]
    public string StoreName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Support email")]
    public string SupportEmail { get; set; } = string.Empty;

    public string? ExistingLogoImagePath { get; set; }

    [Display(Name = "Upload logo")]
    public IFormFile? LogoImageFile { get; set; }

    [Display(Name = "Remove current logo")]
    public bool RemoveLogoImage { get; set; }

    [Display(Name = "Show homepage slider")]
    public bool ShowHomepageSlider { get; set; }

    [Display(Name = "Show homepage categories")]
    public bool ShowHomepageCategories { get; set; }

    [Display(Name = "Show new products section")]
    public bool ShowHomepageNewProducts { get; set; }

    [Display(Name = "Show featured products section")]
    public bool ShowHomepageFeaturedProducts { get; set; }

    [Display(Name = "Show discount products section")]
    public bool ShowHomepageDiscountProducts { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Hero badge text")]
    public string HeroBadgeText { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Hero title")]
    public string HeroTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    [Display(Name = "Hero subtitle")]
    public string HeroSubtitle { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Primary button text")]
    public string HeroPrimaryButtonText { get; set; } = string.Empty;

    [Required]
    [StringLength(260)]
    [Display(Name = "Primary button URL")]
    public string HeroPrimaryButtonUrl { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "Secondary button text")]
    public string? HeroSecondaryButtonText { get; set; }

    [StringLength(260)]
    [Display(Name = "Secondary button URL")]
    public string? HeroSecondaryButtonUrl { get; set; }

    [Display(Name = "Show promo banner")]
    public bool IsPromoBannerActive { get; set; }

    [Required]
    [StringLength(160)]
    [Display(Name = "Promo title")]
    public string PromoTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    [Display(Name = "Promo subtitle")]
    public string PromoSubtitle { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Promo button text")]
    public string PromoButtonText { get; set; } = string.Empty;

    [Required]
    [StringLength(260)]
    [Display(Name = "Promo button URL")]
    public string PromoButtonUrl { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Promo code")]
    public string? PromoCode { get; set; }

    [Range(typeof(decimal), "0", "100")]
    [Display(Name = "Discount percentage")]
    public decimal? PromoDiscountPercentage { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    [Display(Name = "Standard shipping fee")]
    public decimal StandardShippingFee { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    [Display(Name = "Free shipping threshold")]
    public decimal? FreeShippingThreshold { get; set; }

    [Range(typeof(decimal), "0", "100")]
    [Display(Name = "Tax rate (%)")]
    public decimal TaxRatePercentage { get; set; }

    [Required]
    [StringLength(12)]
    [Display(Name = "Currency code")]
    public string CurrencyCode { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Payment instructions")]
    public string? PaymentInstructions { get; set; }

    [StringLength(160)]
    [Display(Name = "SMTP host")]
    public string? SmtpHost { get; set; }

    [Range(1, 65535)]
    [Display(Name = "SMTP port")]
    public int? SmtpPort { get; set; }

    [StringLength(160)]
    [Display(Name = "SMTP username")]
    public string? SmtpUsername { get; set; }

    [StringLength(256)]
    [DataType(DataType.Password)]
    [Display(Name = "SMTP password")]
    public string? SmtpPassword { get; set; }

    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "SMTP from email")]
    public string? SmtpFromEmail { get; set; }

    [StringLength(160)]
    [Display(Name = "SEO meta title")]
    public string? SeoMetaTitle { get; set; }

    [StringLength(320)]
    [Display(Name = "SEO meta description")]
    public string? SeoMetaDescription { get; set; }

    [Required]
    [StringLength(10)]
    [Display(Name = "Default language")]
    public string DefaultCulture { get; set; } = string.Empty;
}
