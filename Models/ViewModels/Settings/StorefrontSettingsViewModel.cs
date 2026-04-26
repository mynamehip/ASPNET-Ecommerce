namespace ASPNET_Ecommerce.Models.ViewModels.Settings;

public class StorefrontSettingsViewModel
{
    public string StoreName { get; set; } = string.Empty;

    public string SupportEmail { get; set; } = string.Empty;

    public string? LogoImagePath { get; set; }

    public bool ShowHomepageSlider { get; set; }

    public bool ShowHomepageCategories { get; set; }

    public bool ShowHomepageNewProducts { get; set; }

    public bool ShowHomepageFeaturedProducts { get; set; }

    public bool ShowHomepageDiscountProducts { get; set; }

    public string HeroBadgeText { get; set; } = string.Empty;

    public string HeroTitle { get; set; } = string.Empty;

    public string HeroSubtitle { get; set; } = string.Empty;

    public string HeroPrimaryButtonText { get; set; } = string.Empty;

    public string HeroPrimaryButtonUrl { get; set; } = string.Empty;

    public string? HeroSecondaryButtonText { get; set; }

    public string? HeroSecondaryButtonUrl { get; set; }

    public bool IsPromoBannerActive { get; set; }

    public string PromoTitle { get; set; } = string.Empty;

    public string PromoSubtitle { get; set; } = string.Empty;

    public string PromoButtonText { get; set; } = string.Empty;

    public string PromoButtonUrl { get; set; } = string.Empty;

    public string? PromoCode { get; set; }

    public decimal? PromoDiscountPercentage { get; set; }

    public decimal StandardShippingFee { get; set; }

    public decimal? FreeShippingThreshold { get; set; }

    public decimal TaxRatePercentage { get; set; }

    public string CurrencyCode { get; set; } = "VND";

    public string? PaymentInstructions { get; set; }

    public string? SmtpHost { get; set; }

    public int? SmtpPort { get; set; }

    public string? SmtpUsername { get; set; }

    public string? SmtpFromEmail { get; set; }

    public string? SeoMetaTitle { get; set; }

    public string? SeoMetaDescription { get; set; }

    public string DefaultCulture { get; set; } = "en";

    public IReadOnlyList<SliderItemViewModel> SliderItems { get; set; } = [];
}
