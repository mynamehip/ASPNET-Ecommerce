using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.Orders;
using ASPNET_Ecommerce.Models.Settings;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<SliderItem> SliderItems => Set<SliderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.Property(category => category.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(category => category.Description)
                .HasMaxLength(1000);

            entity.HasIndex(category => category.Name)
                .IsUnique();
        });

        builder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Name)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(product => product.Sku)
                .HasMaxLength(64);

            entity.Property(product => product.Description)
                .HasMaxLength(4000);

            entity.Property(product => product.Price)
                .HasPrecision(18, 2);

            entity.Property(product => product.DiscountPercentage)
                .HasPrecision(5, 2);

            entity.Property(product => product.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductImage>(entity =>
        {
            entity.Property(image => image.ImagePath)
                .HasMaxLength(260)
                .IsRequired();

            entity.HasOne(image => image.Product)
                .WithMany(product => product.Images)
                .HasForeignKey(image => image.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductReview>(entity =>
        {
            entity.Property(review => review.Comment)
                .HasMaxLength(2000);

            entity.Property(review => review.AdminReply)
                .HasMaxLength(2000);

            entity.Property(review => review.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(review => review.Product)
                .WithMany(product => product.Reviews)
                .HasForeignKey(review => review.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(review => review.User)
                .WithMany()
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(review => new { review.ProductId, review.UserId })
                .IsUnique();
        });

        builder.Entity<WishlistItem>(entity =>
        {
            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(item => new { item.UserId, item.ProductId })
                .IsUnique();
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(order => order.OrderNumber)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(order => order.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(order => order.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(order => order.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(order => order.PaymentProvider)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(order => order.PaymentTransactionReference)
                .HasMaxLength(80);

            entity.Property(order => order.PaymentFailureReason)
                .HasMaxLength(500);

            entity.Property(order => order.ShippingFullName)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(order => order.ShippingEmail)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(order => order.ShippingPhone)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(order => order.ShippingAddress)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(order => order.ShippingWard)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(order => order.ShippingProvince)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(order => order.Notes)
                .HasMaxLength(1000);

            entity.Property(order => order.ShipmentCarrier)
                .HasMaxLength(120);

            entity.Property(order => order.TrackingNumber)
                .HasMaxLength(80);

            entity.Property(order => order.TrackingUrl)
                .HasMaxLength(260);

            entity.Property(order => order.Subtotal)
                .HasPrecision(18, 2);

            entity.Property(order => order.PromoCode)
                .HasMaxLength(40);

            entity.Property(order => order.PromoDiscountPercentage)
                .HasPrecision(5, 2);

            entity.Property(order => order.DiscountAmount)
                .HasPrecision(18, 2);

            entity.Property(order => order.ShippingFee)
                .HasPrecision(18, 2);

            entity.Property(order => order.TaxRate)
                .HasPrecision(5, 2);

            entity.Property(order => order.TaxAmount)
                .HasPrecision(18, 2);

            entity.Property(order => order.CurrencyCode)
                .HasMaxLength(12)
                .IsRequired();

            entity.Property(order => order.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(order => order.CancellationReason)
                .HasMaxLength(500);

            entity.Property(order => order.CancelledByUserId)
                .HasMaxLength(450);

            entity.Property(order => order.CancelledByName)
                .HasMaxLength(120);

            entity.HasIndex(order => order.OrderNumber)
                .IsUnique();

            entity.HasOne(order => order.User)
                .WithMany()
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(order => order.TrackingNumber);
        });

        builder.Entity<OrderStatusHistory>(entity =>
        {
            entity.Property(item => item.PreviousStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(item => item.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(item => item.Note)
                .HasMaxLength(500);

            entity.Property(item => item.ChangedByUserId)
                .HasMaxLength(450);

            entity.Property(item => item.ChangedByName)
                .HasMaxLength(120)
                .IsRequired();

            entity.HasOne(item => item.Order)
                .WithMany(order => order.StatusHistory)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(item => new { item.OrderId, item.ChangedAtUtc });
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.ProductName)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(item => item.ProductSku)
                .HasMaxLength(64);

            entity.Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SystemSetting>(entity =>
        {
            entity.Property(setting => setting.StoreName)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(setting => setting.SupportEmail)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(setting => setting.LogoImagePath)
                .HasMaxLength(260);

            entity.Property(setting => setting.ShowHomepageSlider)
                .HasDefaultValue(true);

            entity.Property(setting => setting.ShowHomepageCategories)
                .HasDefaultValue(true);

            entity.Property(setting => setting.ShowHomepageNewProducts)
                .HasDefaultValue(true);

            entity.Property(setting => setting.ShowHomepageFeaturedProducts)
                .HasDefaultValue(true);

            entity.Property(setting => setting.ShowHomepageDiscountProducts)
                .HasDefaultValue(true);

            entity.Property(setting => setting.HeroBadgeText)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(setting => setting.HeroTitle)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(setting => setting.HeroSubtitle)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(setting => setting.HeroPrimaryButtonText)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(setting => setting.HeroPrimaryButtonUrl)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(setting => setting.HeroSecondaryButtonText)
                .HasMaxLength(80);

            entity.Property(setting => setting.HeroSecondaryButtonUrl)
                .HasMaxLength(260);

            entity.Property(setting => setting.PromoTitle)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(setting => setting.PromoSubtitle)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(setting => setting.PromoButtonText)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(setting => setting.PromoButtonUrl)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(setting => setting.PromoCode)
                .HasMaxLength(40);

            entity.Property(setting => setting.PromoDiscountPercentage)
                .HasPrecision(5, 2);

            entity.Property(setting => setting.StandardShippingFee)
                .HasPrecision(18, 2);

            entity.Property(setting => setting.FreeShippingThreshold)
                .HasPrecision(18, 2);

            entity.Property(setting => setting.TaxRatePercentage)
                .HasPrecision(5, 2);

            entity.Property(setting => setting.CurrencyCode)
                .HasMaxLength(12)
                .IsRequired();

            entity.Property(setting => setting.PaymentInstructions)
                .HasMaxLength(1000);

            entity.Property(setting => setting.SmtpHost)
                .HasMaxLength(160);

            entity.Property(setting => setting.SmtpUsername)
                .HasMaxLength(160);

            entity.Property(setting => setting.SmtpPassword)
                .HasMaxLength(256);

            entity.Property(setting => setting.SmtpFromEmail)
                .HasMaxLength(256);

            entity.Property(setting => setting.SeoMetaTitle)
                .HasMaxLength(160);

            entity.Property(setting => setting.SeoMetaDescription)
                .HasMaxLength(320);

            entity.Property(setting => setting.DefaultCulture)
                .HasMaxLength(10)
                .IsRequired();
        });

        builder.Entity<SliderItem>(entity =>
        {
            entity.Property(item => item.ItemType)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(item => item.Content)
                .HasMaxLength(120);

            entity.Property(item => item.Title)
                .HasMaxLength(200);

            entity.Property(item => item.Description)
                .HasMaxLength(1000);

            entity.Property(item => item.PrimaryButtonUrl)
                .HasMaxLength(260);

            entity.Property(item => item.SecondaryButtonUrl)
                .HasMaxLength(260);

            entity.Property(item => item.BackgroundImagePath)
                .HasMaxLength(260);

            entity.Property(item => item.ClickUrl)
                .HasMaxLength(260);

            entity.HasIndex(item => new { item.IsActive, item.DisplayOrder });
        });
    }
}
