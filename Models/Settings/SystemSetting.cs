using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.Settings;

public class SystemSetting
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string StoreName { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string SupportEmail { get; set; } = string.Empty;

    [StringLength(260)]
    public string? LogoImagePath { get; set; }

    public bool ShowHomepageSlider { get; set; } = true;

    public bool ShowHomepageCategories { get; set; } = true;

    public bool ShowHomepageNewProducts { get; set; } = true;

    public bool ShowHomepageFeaturedProducts { get; set; } = true;

    public bool ShowHomepageDiscountProducts { get; set; } = true;

    [Required]
    [StringLength(120)]
    public string HeroBadgeText { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string HeroTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string HeroSubtitle { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string HeroPrimaryButtonText { get; set; } = string.Empty;

    [Required]
    [StringLength(260)]
    public string HeroPrimaryButtonUrl { get; set; } = string.Empty;

    [StringLength(80)]
    public string? HeroSecondaryButtonText { get; set; }

    [StringLength(260)]
    public string? HeroSecondaryButtonUrl { get; set; }

    public bool IsPromoBannerActive { get; set; } = true;

    [Required]
    [StringLength(160)]
    public string PromoTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string PromoSubtitle { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string PromoButtonText { get; set; } = string.Empty;

    [Required]
    [StringLength(260)]
    public string PromoButtonUrl { get; set; } = string.Empty;

    [StringLength(40)]
    public string? PromoCode { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PromoDiscountPercentage { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal StandardShippingFee { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal? FreeShippingThreshold { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal TaxRatePercentage { get; set; }

    [Required]
    [StringLength(12)]
    public string CurrencyCode { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? PaymentInstructions { get; set; }

    [StringLength(160)]
    public string? SmtpHost { get; set; }

    public int? SmtpPort { get; set; }

    [StringLength(160)]
    public string? SmtpUsername { get; set; }

    [StringLength(256)]
    public string? SmtpPassword { get; set; }

    [StringLength(256)]
    [EmailAddress]
    public string? SmtpFromEmail { get; set; }

    [StringLength(160)]
    public string? SeoMetaTitle { get; set; }

    [StringLength(320)]
    public string? SeoMetaDescription { get; set; }

    [Required]
    [StringLength(10)]
    public string DefaultCulture { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
