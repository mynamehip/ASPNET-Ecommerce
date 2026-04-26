using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.Orders;
using ASPNET_Ecommerce.Models.Settings;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Models.ViewModels.Cart;
using ASPNET_Ecommerce.Models.ViewModels.Order;
using ASPNET_Ecommerce.Models.ViewModels.Products;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace ASPNETEcommerce.Tests;

public class Phase4BusinessFlowTests
{
    [Fact]
    public async Task CreateOrderAsync_AppliesPromoPricing_AndSavesDefaultAddress()
    {
        await using var dbContext = CreateDbContext();
        dbContext.SystemSettings.Add(BuildSettings());
        dbContext.Users.Add(new ApplicationUser
        {
            Id = "user-1",
            UserName = "buyer@example.com",
            Email = "buyer@example.com",
            FullName = "Buyer One"
        });

        dbContext.Categories.Add(new Category { Id = 1, Name = "Category", DisplayOrder = 1, IsActive = true });
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Keyboard",
            CategoryId = 1,
            Price = 100m,
            StockQuantity = 8,
            Status = ProductStatus.Active
        });
        await dbContext.SaveChangesAsync();

        var emailService = new FakeOrderEmailService();
        var service = new OrderService(
            dbContext,
            new FakeCartService(new CartIndexViewModel
            {
                Items = [new CartItemViewModel { ProductId = 1, ProductName = "Keyboard", UnitPrice = 100m, Quantity = 2, AvailableStock = 8 }],
                ItemCount = 2,
                Subtotal = 200m
            }),
            new FakePaymentGatewayService(),
            new FakeRealtimeNotificationService(),
            emailService,
            NullLogger<OrderService>.Instance);

        var orderId = await service.CreateOrderAsync("user-1", new CheckoutViewModel
        {
            FullName = "Buyer One",
            Email = "buyer@example.com",
            PhoneNumber = "0123456789",
            Address = "123 Main St",
            Ward = "Ba Dinh",
            Province = "Hanoi",
            PromoCode = "WELCOME10",
            SaveAsDefaultAddress = true,
            PaymentMethod = PaymentMethod.DemoGateway
        });

        var order = await dbContext.Orders.Include(item => item.StatusHistory).SingleAsync(item => item.Id == orderId);
        var user = await dbContext.Users.SingleAsync(item => item.Id == "user-1");

        Assert.Equal("WELCOME10", order.PromoCode);
        Assert.Equal(20m, order.DiscountAmount);
        Assert.Equal(30000m, order.ShippingFee);
        Assert.Equal(2414.4m, order.TaxAmount);
        Assert.Equal(32594.4m, order.TotalAmount);
        Assert.Equal("VND", order.CurrencyCode);
        Assert.Single(order.StatusHistory);
        Assert.Equal("123 Main St", user.DefaultAddress);
        Assert.Equal("Ba Dinh", user.DefaultWard);
        Assert.Equal("Hanoi", user.DefaultProvince);
        Assert.Equal(order.OrderNumber, emailService.LastOrderNumber);
    }

    [Fact]
    public async Task CreateOrderAsync_AllowsGuestCheckout()
    {
        await using var dbContext = CreateDbContext();
        dbContext.SystemSettings.Add(BuildSettings());
        dbContext.Categories.Add(new Category { Id = 1, Name = "Category", DisplayOrder = 1, IsActive = true });
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Headphones",
            CategoryId = 1,
            Price = 150m,
            StockQuantity = 5,
            Status = ProductStatus.Active
        });
        await dbContext.SaveChangesAsync();

        var emailService = new FakeOrderEmailService();
        var service = new OrderService(
            dbContext,
            new FakeCartService(new CartIndexViewModel
            {
                Items = [new CartItemViewModel { ProductId = 1, ProductName = "Headphones", UnitPrice = 150m, Quantity = 1, AvailableStock = 5 }],
                ItemCount = 1,
                Subtotal = 150m
            }),
            new FakePaymentGatewayService(),
            new FakeRealtimeNotificationService(),
            emailService,
            NullLogger<OrderService>.Instance);

        var orderId = await service.CreateOrderAsync(null, new CheckoutViewModel
        {
            FullName = "Guest Buyer",
            Email = "guest@example.com",
            PhoneNumber = "0123456789",
            Address = "456 Guest St",
            Ward = "Hai Chau",
            Province = "Da Nang",
            PaymentMethod = PaymentMethod.CashOnDelivery
        });

        var order = await dbContext.Orders.Include(item => item.StatusHistory).SingleAsync(item => item.Id == orderId);

        Assert.Null(order.UserId);
        Assert.Equal("Guest Buyer", order.ShippingFullName);
        Assert.Single(order.StatusHistory);
        Assert.Equal(order.OrderNumber, emailService.LastOrderNumber);
    }

    [Fact]
    public async Task CancelAsync_RestoresInventory_AndMarksRefundReady_WhenPaymentWasCaptured()
    {
        await using var dbContext = CreateDbContext();
        dbContext.SystemSettings.Add(BuildSettings());
        dbContext.Users.Add(new ApplicationUser
        {
            Id = "user-1",
            UserName = "buyer@example.com",
            Email = "buyer@example.com",
            FullName = "Buyer One"
        });
        dbContext.Categories.Add(new Category { Id = 1, Name = "Category", DisplayOrder = 1, IsActive = true });
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Mouse",
            CategoryId = 1,
            Price = 80m,
            StockQuantity = 3,
            Status = ProductStatus.Active
        });
        dbContext.Orders.Add(new Order
        {
            Id = 10,
            OrderNumber = "ORD-TEST-10",
            UserId = "user-1",
            ShippingFullName = "Buyer One",
            ShippingEmail = "buyer@example.com",
            ShippingPhone = "0123456789",
            ShippingAddress = "123 Main St",
            ShippingWard = "Ba Dinh",
            ShippingProvince = "Hanoi",
            PaymentProvider = "DemoPay",
            PaymentMethod = PaymentMethod.DemoGateway,
            PaymentStatus = PaymentStatus.Paid,
            Status = OrderStatus.Pending,
            Subtotal = 160m,
            ShippingFee = 30000m,
            TaxRate = 8m,
            TaxAmount = 24172.8m,
            TotalAmount = 54332.8m,
            CurrencyCode = "VND",
            Items = [new OrderItem { ProductId = 1, ProductName = "Mouse", UnitPrice = 80m, Quantity = 2 }],
            StatusHistory = []
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(
            dbContext,
            new FakeCartService(new CartIndexViewModel()),
            new FakePaymentGatewayService(),
            new FakeRealtimeNotificationService(),
            new FakeOrderEmailService(),
            NullLogger<OrderService>.Instance);

        await service.CancelAsync(10, "user-1", "Buyer One", "Changed my mind");

        var order = await dbContext.Orders.Include(item => item.StatusHistory).SingleAsync(item => item.Id == 10);
        var product = await dbContext.Products.SingleAsync(item => item.Id == 1);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.True(order.IsInventoryRestored);
        Assert.True(order.IsRefundReady);
        Assert.Equal(5, product.StockQuantity);
        Assert.Equal("Changed my mind", order.CancellationReason);
        Assert.Single(order.StatusHistory);
    }

    [Fact]
    public async Task LookupAsync_ReturnsGuestOrder_WhenOrderNumberAndEmailMatch()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Orders.Add(new Order
        {
            Id = 21,
            OrderNumber = "ORD-GUEST-21",
            ShippingFullName = "Guest Buyer",
            ShippingEmail = "guest@example.com",
            ShippingPhone = "0123456789",
            ShippingAddress = "456 Guest St",
            ShippingWard = "Hai Chau",
            ShippingProvince = "Da Nang",
            PaymentProvider = "DemoPay",
            PaymentMethod = PaymentMethod.CashOnDelivery,
            PaymentStatus = PaymentStatus.Pending,
            Status = OrderStatus.Pending,
            Subtotal = 150m,
            ShippingFee = 30000m,
            TaxRate = 8m,
            TaxAmount = 2412m,
            TotalAmount = 32562m,
            CurrencyCode = "VND"
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(
            dbContext,
            new FakeCartService(new CartIndexViewModel()),
            new FakePaymentGatewayService(),
            new FakeRealtimeNotificationService(),
            new FakeOrderEmailService(),
            NullLogger<OrderService>.Instance);

        var result = await service.LookupAsync("ORD-GUEST-21", null, "guest@example.com");

        Assert.NotNull(result);
        Assert.Equal(21, result!.OrderId);
        Assert.True(result.IsGuestOrder);
    }

    [Fact]
    public async Task LookupAsync_ReturnsOwnedOrder_ForAuthenticatedUserWithoutEmail()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Orders.Add(new Order
        {
            Id = 22,
            OrderNumber = "ORD-USER-22",
            UserId = "user-22",
            ShippingFullName = "Buyer Two",
            ShippingEmail = "buyer2@example.com",
            ShippingPhone = "0123456789",
            ShippingAddress = "123 Main St",
            ShippingWard = "Ba Dinh",
            ShippingProvince = "Hanoi",
            PaymentProvider = "DemoPay",
            PaymentMethod = PaymentMethod.DemoGateway,
            PaymentStatus = PaymentStatus.Paid,
            Status = OrderStatus.Pending,
            Subtotal = 150m,
            ShippingFee = 30000m,
            TaxRate = 8m,
            TaxAmount = 2412m,
            TotalAmount = 32562m,
            CurrencyCode = "VND"
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(
            dbContext,
            new FakeCartService(new CartIndexViewModel()),
            new FakePaymentGatewayService(),
            new FakeRealtimeNotificationService(),
            new FakeOrderEmailService(),
            NullLogger<OrderService>.Instance);

        var result = await service.LookupAsync("ORD-USER-22", "user-22", null);

        Assert.NotNull(result);
        Assert.Equal(22, result!.OrderId);
        Assert.False(result.IsGuestOrder);
    }

    [Fact]
    public async Task LookupAsync_DoesNotReturnRegisteredOrder_ToGuestEmailLookup()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Orders.Add(new Order
        {
            Id = 23,
            OrderNumber = "ORD-USER-23",
            UserId = "user-23",
            ShippingFullName = "Buyer Three",
            ShippingEmail = "buyer3@example.com",
            ShippingPhone = "0123456789",
            ShippingAddress = "123 Main St",
            ShippingWard = "Ba Dinh",
            ShippingProvince = "Hanoi",
            PaymentProvider = "DemoPay",
            PaymentMethod = PaymentMethod.DemoGateway,
            PaymentStatus = PaymentStatus.Paid,
            Status = OrderStatus.Pending,
            Subtotal = 150m,
            ShippingFee = 30000m,
            TaxRate = 8m,
            TaxAmount = 2412m,
            TotalAmount = 32562m,
            CurrencyCode = "VND"
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(
            dbContext,
            new FakeCartService(new CartIndexViewModel()),
            new FakePaymentGatewayService(),
            new FakeRealtimeNotificationService(),
            new FakeOrderEmailService(),
            NullLogger<OrderService>.Instance);

        var result = await service.LookupAsync("ORD-USER-23", null, "buyer3@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task ReviewService_CreateAsync_RejectsReviewWithoutVerifiedPurchase()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Users.Add(new ApplicationUser { Id = "user-1", UserName = "buyer@example.com", Email = "buyer@example.com", FullName = "Buyer One" });
        dbContext.Categories.Add(new Category { Id = 1, Name = "Category", DisplayOrder = 1, IsActive = true });
        dbContext.Products.Add(new Product { Id = 1, Name = "Desk", CategoryId = 1, Price = 120m, StockQuantity = 4, Status = ProductStatus.Active });
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync("user-1", new ReviewFormViewModel
        {
            ProductId = 1,
            Rating = 5,
            Comment = "Excellent"
        }));

        Assert.Equal("Only verified customers with a completed purchase can review this product.", exception.Message);
    }

    [Fact]
    public async Task ReviewService_CreateAsync_AllowsReviewAfterCompletedPurchase()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Users.Add(new ApplicationUser { Id = "user-1", UserName = "buyer@example.com", Email = "buyer@example.com", FullName = "Buyer One" });
        dbContext.Categories.Add(new Category { Id = 1, Name = "Category", DisplayOrder = 1, IsActive = true });
        dbContext.Products.Add(new Product { Id = 1, Name = "Lamp", CategoryId = 1, Price = 90m, StockQuantity = 4, Status = ProductStatus.Active });
        dbContext.Orders.Add(new Order
        {
            Id = 2,
            OrderNumber = "ORD-TEST-2",
            UserId = "user-1",
            ShippingFullName = "Buyer One",
            ShippingEmail = "buyer@example.com",
            ShippingPhone = "0123456789",
            ShippingAddress = "123 Main St",
            ShippingWard = "Ba Dinh",
            ShippingProvince = "Hanoi",
            PaymentProvider = "DemoPay",
            PaymentMethod = PaymentMethod.DemoGateway,
            PaymentStatus = PaymentStatus.Paid,
            Status = OrderStatus.Completed,
            Subtotal = 90m,
            ShippingFee = 30000m,
            TaxRate = 8m,
            TaxAmount = 24072m,
            TotalAmount = 54162m,
            CurrencyCode = "VND",
            Items = [new OrderItem { ProductId = 1, ProductName = "Lamp", UnitPrice = 90m, Quantity = 1 }]
        });
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        await service.CreateAsync("user-1", new ReviewFormViewModel
        {
            ProductId = 1,
            Rating = 4,
            Comment = "Solid purchase"
        });

        var review = await dbContext.ProductReviews.SingleAsync();
        Assert.Equal(ProductReviewStatus.Approved, review.Status);
        Assert.Equal(4, review.Rating);
    }

    [Fact]
    public async Task ReviewService_UpdateStatusAsync_SavesAdminReply_AndVisibility()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Users.Add(new ApplicationUser { Id = "user-1", UserName = "buyer@example.com", Email = "buyer@example.com", FullName = "Buyer One" });
        dbContext.Categories.Add(new Category { Id = 1, Name = "Category", DisplayOrder = 1, IsActive = true });
        dbContext.Products.Add(new Product { Id = 1, Name = "Lamp", CategoryId = 1, Price = 90m, StockQuantity = 4, Status = ProductStatus.Active });
        dbContext.ProductReviews.Add(new ProductReview
        {
            Id = 1,
            ProductId = 1,
            UserId = "user-1",
            Rating = 5,
            Comment = "Excellent",
            Status = ProductReviewStatus.Approved,
            CreatedAtUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        await service.UpdateStatusAsync(1, new ReviewStatusUpdateViewModel
        {
            ReviewId = 1,
            Status = ProductReviewStatus.Hidden,
            AdminReply = "Thanks for the detailed feedback."
        });

        var review = await dbContext.ProductReviews.SingleAsync();
        Assert.Equal(ProductReviewStatus.Hidden, review.Status);
        Assert.Equal("Thanks for the detailed feedback.", review.AdminReply);
        Assert.NotNull(review.AdminRepliedAtUtc);
        Assert.NotNull(review.UpdatedAtUtc);
    }

    [Fact]
    public async Task WishlistService_AllowsGuestSessionWishlist()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category { Id = 1, Name = "Category", DisplayOrder = 1, IsActive = true });
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Speaker",
            CategoryId = 1,
            Price = 200m,
            StockQuantity = 7,
            Status = ProductStatus.Active
        });
        await dbContext.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature { Session = new TestSession() });

        var service = new WishlistService(dbContext, new HttpContextAccessor { HttpContext = httpContext });

        await service.AddAsync(1);

        var wishlist = await service.GetCurrentAsync();

        Assert.True(await service.ContainsAsync(1));
        Assert.Equal(1, await service.GetItemCountAsync());
        Assert.Single(wishlist.Items);
        Assert.Equal(1, wishlist.Items[0].ProductId);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static SystemSetting BuildSettings()
    {
        return new SystemSetting
        {
            StoreName = "Store",
            SupportEmail = "support@example.com",
            HeroBadgeText = "Badge",
            HeroTitle = "Hero",
            HeroSubtitle = "Subtitle",
            HeroPrimaryButtonText = "Shop",
            HeroPrimaryButtonUrl = "/Product",
            IsPromoBannerActive = true,
            PromoTitle = "Promo",
            PromoSubtitle = "Promo subtitle",
            PromoButtonText = "Browse",
            PromoButtonUrl = "/Product",
            PromoCode = "WELCOME10",
            PromoDiscountPercentage = 10m,
            StandardShippingFee = 30000m,
            FreeShippingThreshold = 500000m,
            TaxRatePercentage = 8m,
            CurrencyCode = "VND",
            DefaultCulture = "en"
        };
    }

    private sealed class FakeCartService : ICartService
    {
        private readonly CartIndexViewModel _cart;

        public FakeCartService(CartIndexViewModel cart)
        {
            _cart = cart;
        }

        public Task<CartIndexViewModel> GetCartAsync(CancellationToken cancellationToken = default) => Task.FromResult(_cart);

        public Task<int> GetItemCountAsync() => Task.FromResult(_cart.ItemCount);

        public Task AddItemAsync(int productId, int quantity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateQuantityAsync(int productId, int quantity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RemoveItemAsync(int productId) => Task.CompletedTask;

        public Task ClearAsync() => Task.CompletedTask;
    }

    private sealed class FakePaymentGatewayService : IPaymentGatewayService
    {
        public Task<PaymentGatewayResult> ProcessAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentGatewayResult
            {
                Succeeded = true,
                Provider = "DemoPay",
                Status = PaymentStatus.Paid,
                TransactionReference = $"TX-{request.OrderNumber}",
                AuthorizedAtUtc = DateTime.UtcNow,
                PaidAtUtc = DateTime.UtcNow
            });
        }
    }

    private sealed class FakeRealtimeNotificationService : IRealtimeNotificationService
    {
        public Task NotifyOrderCreatedAsync(int orderId, string orderNumber, string? userId, string customerName, decimal subtotal, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifyOrderUpdatedAsync(int orderId, string orderNumber, string? userId, OrderStatus status, string? trackingNumber, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeOrderEmailService : IOrderEmailService
    {
        public string? LastOrderNumber { get; private set; }

        public Task TrySendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default)
        {
            LastOrderNumber = order.OrderNumber;
            return Task.CompletedTask;
        }
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = new TestSession();
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _storage = new(StringComparer.Ordinal);

        public bool IsAvailable => true;

        public string Id { get; } = Guid.NewGuid().ToString("N");

        public IEnumerable<string> Keys => _storage.Keys;

        public void Clear() => _storage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _storage.Remove(key);

        public void Set(string key, byte[] value) => _storage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value!);
    }
}