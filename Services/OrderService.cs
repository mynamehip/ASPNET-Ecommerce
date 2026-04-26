using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.Orders;
using ASPNET_Ecommerce.Models.Settings;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Models.ViewModels.Order;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICartService _cartService;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly IOrderEmailService _orderEmailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ApplicationDbContext dbContext,
        ICartService cartService,
        IPaymentGatewayService paymentGatewayService,
        IRealtimeNotificationService realtimeNotificationService,
        IOrderEmailService orderEmailService,
        ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _cartService = cartService;
        _paymentGatewayService = paymentGatewayService;
        _realtimeNotificationService = realtimeNotificationService;
        _orderEmailService = orderEmailService;
        _logger = logger;
    }

    public async Task<CheckoutViewModel> BuildCheckoutAsync(string? userId, CancellationToken cancellationToken = default)
    {
        var model = new CheckoutViewModel
        {
            PaymentMethod = PaymentMethod.CashOnDelivery
        };

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);

            if (user is null)
            {
                throw new InvalidOperationException("User account not found.");
            }

            model.FullName = user.FullName;
            model.Email = user.Email ?? string.Empty;
            model.PhoneNumber = user.PhoneNumber ?? string.Empty;
            model.Address = user.DefaultAddress ?? string.Empty;
            model.Ward = user.DefaultWard ?? string.Empty;
            model.Province = user.DefaultProvince ?? string.Empty;
            model.SaveAsDefaultAddress = !string.IsNullOrWhiteSpace(user.DefaultAddress);
        }

        await PopulateCheckoutAsync(model, cancellationToken);
        return model;
    }

    public async Task PopulateCheckoutAsync(CheckoutViewModel model, CancellationToken cancellationToken = default)
    {
        var cart = await _cartService.GetCartAsync(cancellationToken);
        var settings = await GetStoreSettingsAsync(cancellationToken);
        var pricing = BuildPricingSnapshot(cart.Subtotal, model.PromoCode, settings);

        model.Items = cart.Items;
        model.ItemCount = cart.ItemCount;
        model.Subtotal = cart.Subtotal;
        model.DiscountAmount = pricing.DiscountAmount;
        model.ShippingFee = pricing.ShippingFee;
        model.TaxRatePercentage = pricing.TaxRatePercentage;
        model.TaxAmount = pricing.TaxAmount;
        model.TotalAmount = pricing.TotalAmount;
        model.AppliedPromoCode = pricing.AppliedPromoCode;
        model.AppliedPromoDiscountPercentage = pricing.AppliedPromoDiscountPercentage;
        model.CurrencyCode = settings.CurrencyCode;
        model.FreeShippingThreshold = settings.FreeShippingThreshold;
        model.PaymentInstructions = settings.PaymentInstructions;
    }

    public async Task<int> CreateOrderAsync(string? userId, CheckoutViewModel model, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = null;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            user = await _dbContext.Users
                .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);

            if (user is null)
            {
                throw new InvalidOperationException("User account not found.");
            }
        }

        var cart = await _cartService.GetCartAsync(cancellationToken);
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Your cart is empty.");
        }

        var productIds = cart.Items.Select(item => item.ProductId).ToList();
        var products = await _dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        foreach (var cartItem in cart.Items)
        {
            if (!products.TryGetValue(cartItem.ProductId, out var product) || product.Status != ProductStatus.Active)
            {
                throw new InvalidOperationException($"{cartItem.ProductName} is no longer available.");
            }

            if (product.StockQuantity < cartItem.Quantity)
            {
                throw new InvalidOperationException($"Only {product.StockQuantity} item(s) available for {product.Name}.");
            }
        }

        var settings = await GetStoreSettingsAsync(cancellationToken);
        var pricing = BuildPricingSnapshot(cart.Subtotal, model.PromoCode, settings);
        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);

        var paymentResult = await _paymentGatewayService.ProcessAsync(new PaymentGatewayRequest
        {
            OrderNumber = orderNumber,
            Amount = pricing.TotalAmount,
            Method = model.PaymentMethod,
            CustomerName = model.FullName.Trim(),
            CustomerEmail = model.Email.Trim()
        }, cancellationToken);

        if (!paymentResult.Succeeded)
        {
            throw new InvalidOperationException(paymentResult.FailureReason ?? "Unable to process payment for this order.");
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = user?.Id,
            Status = OrderStatus.Pending,
            PaymentMethod = model.PaymentMethod,
            PaymentStatus = paymentResult.Status,
            PaymentProvider = paymentResult.Provider,
            PaymentTransactionReference = paymentResult.TransactionReference,
            PaymentFailureReason = paymentResult.FailureReason,
            ShippingFullName = model.FullName.Trim(),
            ShippingEmail = model.Email.Trim(),
            ShippingPhone = model.PhoneNumber.Trim(),
            ShippingAddress = model.Address.Trim(),
            ShippingWard = model.Ward.Trim(),
            ShippingProvince = model.Province.Trim(),
            Notes = NormalizeText(model.Notes),
            PromoCode = pricing.AppliedPromoCode,
            PromoDiscountPercentage = pricing.AppliedPromoDiscountPercentage,
            DiscountAmount = pricing.DiscountAmount,
            ShippingFee = pricing.ShippingFee,
            TaxRate = pricing.TaxRatePercentage,
            TaxAmount = pricing.TaxAmount,
            CurrencyCode = settings.CurrencyCode,
            TotalAmount = pricing.TotalAmount,
            Subtotal = cart.Subtotal,
            PaymentAuthorizedAtUtc = paymentResult.AuthorizedAtUtc,
            PaidAtUtc = paymentResult.PaidAtUtc,
            EstimatedDeliveryDateUtc = now.Date.AddDays(4),
            CreatedAtUtc = now,
            Items = [],
            StatusHistory = []
        };

        foreach (var cartItem in cart.Items)
        {
            var product = products[cartItem.ProductId];
            product.StockQuantity -= cartItem.Quantity;
            product.UpdatedAtUtc = now;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity
            });
        }

        var actorDisplayName = !string.IsNullOrWhiteSpace(user?.FullName)
            ? user.FullName
            : model.FullName.Trim();

        AddStatusHistory(order, OrderStatus.Pending, OrderStatus.Pending, BuildOrderCreatedNote(pricing), user?.Id, actorDisplayName, now);

        if (user is not null)
        {
            SaveDefaultAddress(user, model, now);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _cartService.ClearAsync();
        await _realtimeNotificationService.NotifyOrderCreatedAsync(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.ShippingFullName,
            order.TotalAmount,
            cancellationToken);

        try
        {
            await _orderEmailService.TrySendOrderConfirmationAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Order {OrderNumber} was created but the confirmation email pipeline failed unexpectedly.", order.OrderNumber);
        }

        return order.Id;
    }

    public async Task<OrderConfirmationViewModel?> GetConfirmationAsync(int orderId, string? userId, CancellationToken cancellationToken = default)
    {
        var order = await ApplyOrderAccessFilter(
                _dbContext.Orders
                    .AsNoTracking()
                    .Include(item => item.Items),
                orderId,
                userId)
            .SingleOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return null;
        }

        return new OrderConfirmationViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaymentProvider = order.PaymentProvider,
            PaymentTransactionReference = order.PaymentTransactionReference,
            CreatedAtUtc = order.CreatedAtUtc,
            ShippingFullName = order.ShippingFullName,
            ShippingEmail = order.ShippingEmail,
            ShippingPhone = order.ShippingPhone,
            ShippingAddress = order.ShippingAddress,
            ShippingWard = order.ShippingWard,
            ShippingProvince = order.ShippingProvince,
            Notes = order.Notes,
            ShipmentCarrier = order.ShipmentCarrier,
            TrackingNumber = order.TrackingNumber,
            TrackingUrl = order.TrackingUrl,
            EstimatedDeliveryDateUtc = order.EstimatedDeliveryDateUtc,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            PromoCode = order.PromoCode,
            DiscountAmount = order.DiscountAmount,
            ShippingFee = order.ShippingFee,
            TaxRate = order.TaxRate,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode,
            IsRefundReady = order.IsRefundReady,
            RefundReadyAtUtc = order.RefundReadyAtUtc,
            Items = order.Items
                .Select(item => new OrderConfirmationItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductSku = item.ProductSku,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                })
                .ToList(),
            ItemCount = order.Items.Sum(item => item.Quantity),
            Subtotal = order.Subtotal
        };
    }

    public async Task<IReadOnlyList<OrderHistoryItemViewModel>> GetHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => new OrderHistoryItemViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                TrackingNumber = order.TrackingNumber,
                CreatedAtUtc = order.CreatedAtUtc,
                ItemCount = order.Items.Sum(item => item.Quantity),
                Subtotal = order.Subtotal,
                TotalAmount = order.TotalAmount,
                IsRefundReady = order.IsRefundReady
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDetailsViewModel?> GetDetailsAsync(int orderId, string? userId, CancellationToken cancellationToken = default)
    {
        var order = await ApplyOrderAccessFilter(
                _dbContext.Orders
                    .AsNoTracking()
                    .Include(item => item.Items)
                    .Include(item => item.StatusHistory),
                orderId,
                userId)
            .SingleOrDefaultAsync(cancellationToken);

        return order is null ? null : MapOrderDetails(order);
    }

    public async Task<OrderTrackingViewModel?> GetTrackingAsync(int orderId, string? userId, CancellationToken cancellationToken = default)
    {
        var order = await ApplyOrderAccessFilter(
                _dbContext.Orders.AsNoTracking(),
                orderId,
                userId)
            .SingleOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return null;
        }

        return new OrderTrackingViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaymentProvider = order.PaymentProvider,
            PaymentTransactionReference = order.PaymentTransactionReference,
            CreatedAtUtc = order.CreatedAtUtc,
            ShippingFullName = order.ShippingFullName,
            ShippingWard = order.ShippingWard,
            ShippingProvince = order.ShippingProvince,
            ShipmentCarrier = order.ShipmentCarrier,
            TrackingNumber = order.TrackingNumber,
            TrackingUrl = order.TrackingUrl,
            EstimatedDeliveryDateUtc = order.EstimatedDeliveryDateUtc,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            IsGuestOrder = string.IsNullOrWhiteSpace(order.UserId)
        };
    }

    public async Task<OrderLookupResultViewModel?> LookupAsync(string orderNumber, string? userId, string? email, CancellationToken cancellationToken = default)
    {
        var normalizedOrderNumber = NormalizeText(orderNumber)?.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedOrderNumber))
        {
            return null;
        }

        var order = await _dbContext.Orders
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.OrderNumber == normalizedOrderNumber, cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(userId) && string.Equals(order.UserId, userId, StringComparison.Ordinal))
        {
            return new OrderLookupResultViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                IsGuestOrder = string.IsNullOrWhiteSpace(order.UserId)
            };
        }

        var normalizedEmail = NormalizeText(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail)
            || !string.IsNullOrWhiteSpace(order.UserId)
            || !string.Equals(order.ShippingEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new OrderLookupResultViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            IsGuestOrder = true
        };
    }

    public async Task<OrderAdminListViewModel> GetAdminOrderListAsync(string? search, OrderStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(order =>
                order.OrderNumber.Contains(term) ||
                order.ShippingFullName.Contains(term) ||
                order.ShippingEmail.Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        var orders = await query
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => new OrderAdminListItemViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.ShippingFullName,
                CustomerEmail = order.ShippingEmail,
                Status = order.Status,
                CreatedAtUtc = order.CreatedAtUtc,
                ItemCount = order.Items.Sum(item => item.Quantity),
                Subtotal = order.TotalAmount
            })
            .ToListAsync(cancellationToken);

        return new OrderAdminListViewModel
        {
            Search = search,
            Status = status,
            Orders = orders
        };
    }

    public async Task<OrderAdminDetailsViewModel?> GetAdminDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .Include(item => item.StatusHistory)
            .SingleOrDefaultAsync(item => item.Id == orderId, cancellationToken);

        if (order is null)
        {
            return null;
        }

        return new OrderAdminDetailsViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            CustomerName = order.ShippingFullName,
            CustomerEmail = order.ShippingEmail,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaymentProvider = order.PaymentProvider,
            PaymentTransactionReference = order.PaymentTransactionReference,
            PaymentAuthorizedAtUtc = order.PaymentAuthorizedAtUtc,
            PaidAtUtc = order.PaidAtUtc,
            CreatedAtUtc = order.CreatedAtUtc,
            UpdatedAtUtc = order.UpdatedAtUtc,
            ShippingFullName = order.ShippingFullName,
            ShippingEmail = order.ShippingEmail,
            ShippingPhone = order.ShippingPhone,
            ShippingAddress = order.ShippingAddress,
            ShippingWard = order.ShippingWard,
            ShippingProvince = order.ShippingProvince,
            Notes = order.Notes,
            ShipmentCarrier = order.ShipmentCarrier,
            TrackingNumber = order.TrackingNumber,
            TrackingUrl = order.TrackingUrl,
            EstimatedDeliveryDateUtc = order.EstimatedDeliveryDateUtc,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            PromoCode = order.PromoCode,
            DiscountAmount = order.DiscountAmount,
            ShippingFee = order.ShippingFee,
            TaxRate = order.TaxRate,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode,
            CancelledAtUtc = order.CancelledAtUtc,
            CancellationReason = order.CancellationReason,
            CancelledByName = order.CancelledByName,
            IsRefundReady = order.IsRefundReady,
            RefundReadyAtUtc = order.RefundReadyAtUtc,
            StatusHistory = MapStatusHistory(order),
            Items = order.Items
                .Select(item => new OrderConfirmationItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductSku = item.ProductSku,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                })
                .ToList(),
            ItemCount = order.Items.Sum(item => item.Quantity),
            Subtotal = order.Subtotal,
            StatusForm = new OrderStatusUpdateViewModel
            {
                OrderId = order.Id,
                Status = order.Status,
                ShipmentCarrier = order.ShipmentCarrier,
                TrackingNumber = order.TrackingNumber,
                TrackingUrl = order.TrackingUrl,
                EstimatedDeliveryDateUtc = order.EstimatedDeliveryDateUtc?.Date,
                CancellationReason = order.CancellationReason
            }
        };
    }

    public async Task UpdateStatusAsync(int orderId, OrderStatusUpdateViewModel model, string? actorUserId, string actorDisplayName, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(model.Status))
        {
            throw new InvalidOperationException("Invalid order status.");
        }

        var order = await _dbContext.Orders
            .Include(item => item.Items)
            .Include(item => item.StatusHistory)
            .SingleOrDefaultAsync(item => item.Id == orderId, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        var now = DateTime.UtcNow;
        var previousStatus = order.Status;
        var shipmentCarrier = NormalizeText(model.ShipmentCarrier);
        var trackingNumber = NormalizeText(model.TrackingNumber);
        var trackingUrl = NormalizeText(model.TrackingUrl);
        var estimatedDeliveryDateUtc = NormalizeDate(model.EstimatedDeliveryDateUtc);
        var statusNote = NormalizeText(model.StatusNote);
        var cancellationReason = NormalizeText(model.CancellationReason);
        var hasChanges = false;

        if (model.Status == OrderStatus.Cancelled)
        {
            await CancelOrderInternalAsync(order, actorUserId, actorDisplayName, cancellationReason ?? statusNote, now, cancellationToken);
            hasChanges = true;
        }
        else
        {
            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Cancelled orders cannot be moved back into the active workflow.");
            }

            if (order.Status != model.Status)
            {
                order.Status = model.Status;
                hasChanges = true;
            }

            if (!string.Equals(order.ShipmentCarrier, shipmentCarrier, StringComparison.Ordinal))
            {
                order.ShipmentCarrier = shipmentCarrier;
                hasChanges = true;
            }

            if (!string.Equals(order.TrackingNumber, trackingNumber, StringComparison.Ordinal))
            {
                order.TrackingNumber = trackingNumber;
                hasChanges = true;
            }

            if (!string.Equals(order.TrackingUrl, trackingUrl, StringComparison.Ordinal))
            {
                order.TrackingUrl = trackingUrl;
                hasChanges = true;
            }

            if (order.EstimatedDeliveryDateUtc != estimatedDeliveryDateUtc)
            {
                order.EstimatedDeliveryDateUtc = estimatedDeliveryDateUtc;
                hasChanges = true;
            }

            if (order.Status == OrderStatus.Processing && !order.ShippedAtUtc.HasValue)
            {
                order.ShippedAtUtc = now;
                hasChanges = true;
            }

            if (order.Status == OrderStatus.Completed)
            {
                if (!order.ShippedAtUtc.HasValue)
                {
                    order.ShippedAtUtc = now;
                    hasChanges = true;
                }

                if (!order.DeliveredAtUtc.HasValue)
                {
                    order.DeliveredAtUtc = now;
                    hasChanges = true;
                }

                if (order.PaymentMethod == PaymentMethod.CashOnDelivery && order.PaymentStatus == PaymentStatus.Pending)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.PaidAtUtc = now;
                    hasChanges = true;
                }

                order.IsRefundReady = false;
                order.RefundReadyAtUtc = null;
            }

            if (hasChanges)
            {
                AddStatusHistory(order, previousStatus, order.Status, statusNote, actorUserId, actorDisplayName, now);
            }
        }

        if (!hasChanges)
        {
            return;
        }

        order.UpdatedAtUtc = now;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _realtimeNotificationService.NotifyOrderUpdatedAsync(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.Status,
            order.TrackingNumber,
            cancellationToken);
    }

    public async Task CancelAsync(int orderId, string userId, string actorDisplayName, string? reason, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(item => item.Items)
            .Include(item => item.StatusHistory)
            .SingleOrDefaultAsync(item => item.Id == orderId && item.UserId == userId, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        var now = DateTime.UtcNow;
        await CancelOrderInternalAsync(order, userId, actorDisplayName, NormalizeText(reason), now, cancellationToken);
        order.UpdatedAtUtc = now;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _realtimeNotificationService.NotifyOrderUpdatedAsync(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.Status,
            order.TrackingNumber,
            cancellationToken);
    }

    private async Task CancelOrderInternalAsync(Order order, string? actorUserId, string actorDisplayName, string? reason, DateTime timestampUtc, CancellationToken cancellationToken)
    {
        if (!CanBeCancelled(order))
        {
            throw new InvalidOperationException("This order can no longer be cancelled.");
        }

        if (!order.IsInventoryRestored)
        {
            var productIds = order.Items.Select(item => item.ProductId).Distinct().ToList();
            var products = await _dbContext.Products
                .Where(product => productIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id, cancellationToken);

            foreach (var item in order.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    continue;
                }

                product.StockQuantity += item.Quantity;
                product.UpdatedAtUtc = timestampUtc;
            }

            order.IsInventoryRestored = true;
        }

        var previousStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.CancelledAtUtc = timestampUtc;
        order.CancellationReason = reason;
        order.CancelledByUserId = actorUserId;
        order.CancelledByName = actorDisplayName;
        order.TrackingNumber = null;
        order.TrackingUrl = null;
        order.ShipmentCarrier = null;
        order.EstimatedDeliveryDateUtc = null;

        if (order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Authorized)
        {
            order.IsRefundReady = true;
            order.RefundReadyAtUtc ??= timestampUtc;
        }

        AddStatusHistory(order, previousStatus, OrderStatus.Cancelled, reason, actorUserId, actorDisplayName, timestampUtc);
    }

    private static bool CanBeCancelled(Order order)
    {
        return order.Status != OrderStatus.Cancelled
            && order.Status != OrderStatus.Completed
            && !order.ShippedAtUtc.HasValue;
    }

    private static void SaveDefaultAddress(ApplicationUser user, CheckoutViewModel model, DateTime timestampUtc)
    {
        if (!model.SaveAsDefaultAddress)
        {
            return;
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber.Trim();
        user.DefaultAddress = model.Address.Trim();
        user.DefaultWard = model.Ward.Trim();
        user.DefaultProvince = model.Province.Trim();
    }

    private async Task<SystemSetting> GetStoreSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.SystemSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            throw new InvalidOperationException("Store settings have not been initialized.");
        }

        return settings;
    }

    private static PricingSnapshot BuildPricingSnapshot(decimal subtotal, string? promoCodeInput, SystemSetting settings)
    {
        var normalizedPromoCode = NormalizeText(promoCodeInput)?.ToUpperInvariant();
        decimal? appliedPercentage = null;
        string? appliedCode = null;

        if (!string.IsNullOrWhiteSpace(normalizedPromoCode))
        {
            if (string.IsNullOrWhiteSpace(settings.PromoCode) || !settings.PromoDiscountPercentage.HasValue)
            {
                throw new InvalidOperationException("Promo code is currently unavailable.");
            }

            if (!string.Equals(settings.PromoCode, normalizedPromoCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Promo code is invalid.");
            }

            appliedCode = settings.PromoCode;
            appliedPercentage = settings.PromoDiscountPercentage.Value;
        }

        var discountAmount = appliedPercentage.HasValue
            ? Math.Round(subtotal * (appliedPercentage.Value / 100m), 2, MidpointRounding.AwayFromZero)
            : 0m;

        var discountedSubtotal = Math.Max(0m, subtotal - discountAmount);
        var shippingFee = settings.FreeShippingThreshold.HasValue && discountedSubtotal >= settings.FreeShippingThreshold.Value
            ? 0m
            : settings.StandardShippingFee;
        var taxAmount = Math.Round((discountedSubtotal + shippingFee) * (settings.TaxRatePercentage / 100m), 2, MidpointRounding.AwayFromZero);
        var totalAmount = discountedSubtotal + shippingFee + taxAmount;

        return new PricingSnapshot(
            appliedCode,
            appliedPercentage,
            discountAmount,
            shippingFee,
            settings.TaxRatePercentage,
            taxAmount,
            totalAmount);
    }

    private static string BuildOrderCreatedNote(PricingSnapshot pricing)
    {
        if (!string.IsNullOrWhiteSpace(pricing.AppliedPromoCode))
        {
            return $"Order created with promo {pricing.AppliedPromoCode} and total recalculated.";
        }

        return "Order created and queued for fulfillment.";
    }

    private static void AddStatusHistory(Order order, OrderStatus previousStatus, OrderStatus newStatus, string? note, string? actorUserId, string actorDisplayName, DateTime changedAtUtc)
    {
        order.StatusHistory.Add(new OrderStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Note = note,
            ChangedByUserId = actorUserId,
            ChangedByName = string.IsNullOrWhiteSpace(actorDisplayName) ? "System" : actorDisplayName.Trim(),
            ChangedAtUtc = changedAtUtc
        });
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            var exists = await _dbContext.Orders
                .AsNoTracking()
                .AnyAsync(order => order.OrderNumber == candidate, cancellationToken);

            if (!exists)
            {
                return candidate;
            }
        }

        return $"ORD-{Guid.NewGuid():N}"[..32].ToUpperInvariant();
    }

    private static IQueryable<Order> ApplyOrderAccessFilter(IQueryable<Order> query, int orderId, string? userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return query.Where(item => item.Id == orderId && item.UserId == userId);
        }

        return query.Where(item => item.Id == orderId && item.UserId == null);
    }

    private static OrderDetailsViewModel MapOrderDetails(Order order)
    {
        return new OrderDetailsViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaymentProvider = order.PaymentProvider,
            PaymentTransactionReference = order.PaymentTransactionReference,
            CreatedAtUtc = order.CreatedAtUtc,
            UpdatedAtUtc = order.UpdatedAtUtc,
            ShippingFullName = order.ShippingFullName,
            ShippingEmail = order.ShippingEmail,
            ShippingPhone = order.ShippingPhone,
            ShippingAddress = order.ShippingAddress,
            ShippingWard = order.ShippingWard,
            ShippingProvince = order.ShippingProvince,
            Notes = order.Notes,
            ShipmentCarrier = order.ShipmentCarrier,
            TrackingNumber = order.TrackingNumber,
            TrackingUrl = order.TrackingUrl,
            EstimatedDeliveryDateUtc = order.EstimatedDeliveryDateUtc,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            PromoCode = order.PromoCode,
            DiscountAmount = order.DiscountAmount,
            ShippingFee = order.ShippingFee,
            TaxRate = order.TaxRate,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode,
            CancelledAtUtc = order.CancelledAtUtc,
            CancellationReason = order.CancellationReason,
            CancelledByName = order.CancelledByName,
            IsRefundReady = order.IsRefundReady,
            RefundReadyAtUtc = order.RefundReadyAtUtc,
            CanBeCancelled = CanBeCancelled(order),
            IsGuestOrder = string.IsNullOrWhiteSpace(order.UserId),
            StatusHistory = MapStatusHistory(order),
            Items = order.Items
                .Select(item => new OrderConfirmationItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductSku = item.ProductSku,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                })
                .ToList(),
            ItemCount = order.Items.Sum(item => item.Quantity),
            Subtotal = order.Subtotal
        };
    }

    private static IReadOnlyList<OrderStatusHistoryItemViewModel> MapStatusHistory(Order order)
    {
        return order.StatusHistory
            .OrderByDescending(item => item.ChangedAtUtc)
            .Select(item => new OrderStatusHistoryItemViewModel
            {
                PreviousStatus = item.PreviousStatus,
                NewStatus = item.NewStatus,
                Note = item.Note,
                ChangedByName = item.ChangedByName,
                ChangedAtUtc = item.ChangedAtUtc
            })
            .ToList();
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTime? NormalizeDate(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);
    }

    private sealed record PricingSnapshot(
        string? AppliedPromoCode,
        decimal? AppliedPromoDiscountPercentage,
        decimal DiscountAmount,
        decimal ShippingFee,
        decimal TaxRatePercentage,
        decimal TaxAmount,
        decimal TotalAmount);
}
