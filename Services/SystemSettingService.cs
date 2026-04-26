using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Settings;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Models.ViewModels.Settings;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class SystemSettingService : ISystemSettingService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".svg"
    };

    private const long MaxLogoSizeBytes = 2 * 1024 * 1024;
    private const string UploadFolder = "uploads/settings";

    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public SystemSettingService(ApplicationDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task EnsureDefaultAsync(CancellationToken cancellationToken = default)
    {
        await GetOrCreateAsync(cancellationToken);
    }

    public async Task<StorefrontSettingsViewModel> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        var entity = await GetOrCreateAsync(cancellationToken);

        return new StorefrontSettingsViewModel
        {
            StoreName = entity.StoreName,
            SupportEmail = entity.SupportEmail,
            LogoImagePath = entity.LogoImagePath,
            ShowHomepageSlider = entity.ShowHomepageSlider,
            ShowHomepageCategories = entity.ShowHomepageCategories,
            ShowHomepageNewProducts = entity.ShowHomepageNewProducts,
            ShowHomepageFeaturedProducts = entity.ShowHomepageFeaturedProducts,
            ShowHomepageDiscountProducts = entity.ShowHomepageDiscountProducts,
            HeroBadgeText = entity.HeroBadgeText,
            HeroTitle = entity.HeroTitle,
            HeroSubtitle = entity.HeroSubtitle,
            HeroPrimaryButtonText = entity.HeroPrimaryButtonText,
            HeroPrimaryButtonUrl = entity.HeroPrimaryButtonUrl,
            HeroSecondaryButtonText = entity.HeroSecondaryButtonText,
            HeroSecondaryButtonUrl = entity.HeroSecondaryButtonUrl,
            IsPromoBannerActive = entity.IsPromoBannerActive,
            PromoTitle = entity.PromoTitle,
            PromoSubtitle = entity.PromoSubtitle,
            PromoButtonText = entity.PromoButtonText,
            PromoButtonUrl = entity.PromoButtonUrl,
            PromoCode = entity.PromoCode,
            PromoDiscountPercentage = entity.PromoDiscountPercentage,
            StandardShippingFee = entity.StandardShippingFee,
            FreeShippingThreshold = entity.FreeShippingThreshold,
            TaxRatePercentage = entity.TaxRatePercentage,
            CurrencyCode = entity.CurrencyCode,
            PaymentInstructions = entity.PaymentInstructions,
            SmtpHost = entity.SmtpHost,
            SmtpPort = entity.SmtpPort,
            SmtpUsername = entity.SmtpUsername,
            SmtpFromEmail = entity.SmtpFromEmail,
            SeoMetaTitle = entity.SeoMetaTitle,
            SeoMetaDescription = entity.SeoMetaDescription,
            DefaultCulture = entity.DefaultCulture,
            SliderItems = await GetActiveSliderItemsAsync(cancellationToken)
        };
    }

    public async Task<IReadOnlyList<SliderItemViewModel>> GetActiveSliderItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SliderItems
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Id)
            .Select(item => new SliderItemViewModel
            {
                Id = item.Id,
                ItemType = item.ItemType,
                Content = item.Content,
                Title = item.Title,
                Description = item.Description,
                PrimaryButtonUrl = item.PrimaryButtonUrl,
                SecondaryButtonUrl = item.SecondaryButtonUrl,
                BackgroundImagePath = item.BackgroundImagePath,
                ClickUrl = item.ClickUrl,
                IsActive = item.IsActive,
                DisplayOrder = item.DisplayOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SettingFormViewModel> GetForEditAsync(CancellationToken cancellationToken = default)
    {
        var entity = await GetOrCreateAsync(cancellationToken);

        return new SettingFormViewModel
        {
            Id = entity.Id,
            StoreName = entity.StoreName,
            SupportEmail = entity.SupportEmail,
            ExistingLogoImagePath = entity.LogoImagePath,
            ShowHomepageSlider = entity.ShowHomepageSlider,
            ShowHomepageCategories = entity.ShowHomepageCategories,
            ShowHomepageNewProducts = entity.ShowHomepageNewProducts,
            ShowHomepageFeaturedProducts = entity.ShowHomepageFeaturedProducts,
            ShowHomepageDiscountProducts = entity.ShowHomepageDiscountProducts,
            HeroBadgeText = entity.HeroBadgeText,
            HeroTitle = entity.HeroTitle,
            HeroSubtitle = entity.HeroSubtitle,
            HeroPrimaryButtonText = entity.HeroPrimaryButtonText,
            HeroPrimaryButtonUrl = entity.HeroPrimaryButtonUrl,
            HeroSecondaryButtonText = entity.HeroSecondaryButtonText,
            HeroSecondaryButtonUrl = entity.HeroSecondaryButtonUrl,
            IsPromoBannerActive = entity.IsPromoBannerActive,
            PromoTitle = entity.PromoTitle,
            PromoSubtitle = entity.PromoSubtitle,
            PromoButtonText = entity.PromoButtonText,
            PromoButtonUrl = entity.PromoButtonUrl,
            PromoCode = entity.PromoCode,
            PromoDiscountPercentage = entity.PromoDiscountPercentage,
            StandardShippingFee = entity.StandardShippingFee,
            FreeShippingThreshold = entity.FreeShippingThreshold,
            TaxRatePercentage = entity.TaxRatePercentage,
            CurrencyCode = entity.CurrencyCode,
            PaymentInstructions = entity.PaymentInstructions,
            SmtpHost = entity.SmtpHost,
            SmtpPort = entity.SmtpPort,
            SmtpUsername = entity.SmtpUsername,
            SmtpFromEmail = entity.SmtpFromEmail,
            SeoMetaTitle = entity.SeoMetaTitle,
            SeoMetaDescription = entity.SeoMetaDescription,
            DefaultCulture = entity.DefaultCulture
        };
    }

    public async Task UpdateAsync(SettingFormViewModel model, CancellationToken cancellationToken = default)
    {
        var entity = await GetOrCreateAsync(cancellationToken);
        var previousLogoPath = entity.LogoImagePath;

        entity.StoreName = model.StoreName.Trim();
        entity.SupportEmail = model.SupportEmail.Trim();
        entity.ShowHomepageSlider = model.ShowHomepageSlider;
        entity.ShowHomepageCategories = model.ShowHomepageCategories;
        entity.ShowHomepageNewProducts = model.ShowHomepageNewProducts;
        entity.ShowHomepageFeaturedProducts = model.ShowHomepageFeaturedProducts;
        entity.ShowHomepageDiscountProducts = model.ShowHomepageDiscountProducts;
        entity.HeroBadgeText = model.HeroBadgeText.Trim();
        entity.HeroTitle = model.HeroTitle.Trim();
        entity.HeroSubtitle = model.HeroSubtitle.Trim();
        entity.HeroPrimaryButtonText = model.HeroPrimaryButtonText.Trim();
        entity.HeroPrimaryButtonUrl = model.HeroPrimaryButtonUrl.Trim();
        entity.HeroSecondaryButtonText = string.IsNullOrWhiteSpace(model.HeroSecondaryButtonText) ? null : model.HeroSecondaryButtonText.Trim();
        entity.HeroSecondaryButtonUrl = string.IsNullOrWhiteSpace(model.HeroSecondaryButtonUrl) ? null : model.HeroSecondaryButtonUrl.Trim();
        entity.IsPromoBannerActive = model.IsPromoBannerActive;
        entity.PromoTitle = model.PromoTitle.Trim();
        entity.PromoSubtitle = model.PromoSubtitle.Trim();
        entity.PromoButtonText = model.PromoButtonText.Trim();
        entity.PromoButtonUrl = model.PromoButtonUrl.Trim();
        entity.PromoCode = string.IsNullOrWhiteSpace(model.PromoCode) ? null : model.PromoCode.Trim().ToUpperInvariant();
        entity.PromoDiscountPercentage = model.PromoDiscountPercentage;
        entity.StandardShippingFee = model.StandardShippingFee;
        entity.FreeShippingThreshold = model.FreeShippingThreshold;
        entity.TaxRatePercentage = model.TaxRatePercentage;
        entity.CurrencyCode = model.CurrencyCode.Trim().ToUpperInvariant();
        entity.PaymentInstructions = string.IsNullOrWhiteSpace(model.PaymentInstructions) ? null : model.PaymentInstructions.Trim();
        entity.SmtpHost = string.IsNullOrWhiteSpace(model.SmtpHost) ? null : model.SmtpHost.Trim();
        entity.SmtpPort = model.SmtpPort;
        entity.SmtpUsername = string.IsNullOrWhiteSpace(model.SmtpUsername) ? null : model.SmtpUsername.Trim();
        if (!string.IsNullOrWhiteSpace(model.SmtpPassword))
        {
            entity.SmtpPassword = model.SmtpPassword.Trim();
        }
        entity.SmtpFromEmail = string.IsNullOrWhiteSpace(model.SmtpFromEmail) ? null : model.SmtpFromEmail.Trim();
        entity.SeoMetaTitle = string.IsNullOrWhiteSpace(model.SeoMetaTitle) ? null : model.SeoMetaTitle.Trim();
        entity.SeoMetaDescription = string.IsNullOrWhiteSpace(model.SeoMetaDescription) ? null : model.SeoMetaDescription.Trim();
        entity.DefaultCulture = model.DefaultCulture.Trim().ToLowerInvariant();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        if (model.RemoveLogoImage)
        {
            entity.LogoImagePath = null;
        }

        if (model.LogoImageFile is not null && model.LogoImageFile.Length > 0)
        {
            entity.LogoImagePath = await SaveLogoImageAsync(model.LogoImageFile, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousLogoPath) && previousLogoPath != entity.LogoImagePath)
        {
            DeletePhysicalFile(previousLogoPath);
        }
    }

    private async Task<SystemSetting> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var entity = await _dbContext.SystemSettings.SingleOrDefaultAsync(cancellationToken);
        if (entity is not null)
        {
            await EnsureDefaultSliderSeedAsync(entity, cancellationToken);
            return entity;
        }

        entity = new SystemSetting
        {
            StoreName = "EcomShop",
            SupportEmail = "support@aspnetecommerce.local",
            ShowHomepageSlider = true,
            ShowHomepageCategories = true,
            ShowHomepageNewProducts = true,
            ShowHomepageFeaturedProducts = true,
            ShowHomepageDiscountProducts = true,
            HeroBadgeText = "Free shipping on orders over 500K",
            HeroTitle = "Discover the Best Products for You",
            HeroSubtitle = "Shop from our curated collection of high-quality products at unbeatable prices.",
            HeroPrimaryButtonText = "Shop Now",
            HeroPrimaryButtonUrl = "/Product",
            HeroSecondaryButtonText = "New Arrivals",
            HeroSecondaryButtonUrl = "/Product?sortBy=newest",
            IsPromoBannerActive = true,
            PromoTitle = "Ready to Start Shopping?",
            PromoSubtitle = "Create an account today and get access to exclusive deals and promotions.",
            PromoButtonText = "Browse Products",
            PromoButtonUrl = "/Product",
            PromoCode = "WELCOME10",
            PromoDiscountPercentage = 10,
            StandardShippingFee = 30000,
            FreeShippingThreshold = 500000,
            TaxRatePercentage = 8,
            CurrencyCode = "VND",
            PaymentInstructions = "Cash on delivery is collected at delivery. DemoPay settles immediately for testing and sandbox checkout verification.",
            SmtpHost = "smtp.example.local",
            SmtpPort = 587,
            SmtpUsername = "notifications@example.local",
            SmtpPassword = null,
            SmtpFromEmail = "support@aspnetecommerce.local",
            SeoMetaTitle = "ASP.NET Ecommerce Storefront",
            SeoMetaDescription = "Browse products, place orders, and manage your account in the ASP.NET Ecommerce storefront.",
            DefaultCulture = "en",
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.SystemSettings.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await EnsureDefaultSliderSeedAsync(entity, cancellationToken);
        return entity;
    }

    private async Task EnsureDefaultSliderSeedAsync(SystemSetting entity, CancellationToken cancellationToken)
    {
        if (await _dbContext.SliderItems.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.SliderItems.Add(new SliderItem
        {
            ItemType = SliderItemType.Banner,
            Content = entity.HeroBadgeText,
            Title = entity.HeroTitle,
            Description = entity.HeroSubtitle,
            PrimaryButtonUrl = entity.HeroPrimaryButtonUrl,
            SecondaryButtonUrl = entity.HeroSecondaryButtonUrl,
            IsActive = true,
            DisplayOrder = 0,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> SaveLogoImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ValidateLogoImage(file);

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            throw new InvalidOperationException("Web root path is not configured for logo upload.");
        }

        var uploadRoot = Path.Combine(webRootPath, "uploads", "settings");
        Directory.CreateDirectory(uploadRoot);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"logo-{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadRoot, fileName);

        await using var stream = File.Create(physicalPath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/{UploadFolder}/{fileName}".Replace("\\", "/");
    }

    private static void ValidateLogoImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only .jpg, .jpeg, .png, .webp, and .svg files are allowed for the logo.");
        }

        if (file.Length > MaxLogoSizeBytes)
        {
            throw new InvalidOperationException("The logo image must be 2 MB or smaller.");
        }
    }

    private void DeletePhysicalFile(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            return;
        }

        var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }
}
