using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Services;

public interface IRealtimeNotificationService
{
    Task NotifyOrderCreatedAsync(int orderId, string orderNumber, string? userId, string customerName, decimal subtotal, CancellationToken cancellationToken = default);

    Task NotifyOrderUpdatedAsync(int orderId, string orderNumber, string? userId, OrderStatus status, string? trackingNumber, CancellationToken cancellationToken = default);
}