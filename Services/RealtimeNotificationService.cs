using ASPNET_Ecommerce.Hubs;
using ASPNET_Ecommerce.Models.Orders;
using Microsoft.AspNetCore.SignalR;

namespace ASPNET_Ecommerce.Services;

public class RealtimeNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RealtimeNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyOrderCreatedAsync(int orderId, string orderNumber, string? userId, string customerName, decimal subtotal, CancellationToken cancellationToken = default)
    {
        var adminPayload = new
        {
            type = "order.created",
            orderId,
            orderNumber,
            customerName,
            subtotal,
            createdAtUtc = DateTime.UtcNow,
            message = $"New order {orderNumber} was placed by {customerName}."
        };

        var userPayload = new
        {
            type = "order.created",
            orderId,
            orderNumber,
            subtotal,
            createdAtUtc = DateTime.UtcNow,
            message = $"Your order {orderNumber} was placed successfully."
        };

        await _hubContext.Clients.Group(NotificationHub.AdminGroup).SendAsync("notification", adminPayload, cancellationToken);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await _hubContext.Clients.Group(NotificationHub.UserGroup(userId)).SendAsync("notification", userPayload, cancellationToken);
        }
    }

    public async Task NotifyOrderUpdatedAsync(int orderId, string orderNumber, string? userId, OrderStatus status, string? trackingNumber, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            type = "order.updated",
            orderId,
            orderNumber,
            status = status.ToString(),
            trackingNumber,
            updatedAtUtc = DateTime.UtcNow,
            message = trackingNumber is { Length: > 0 }
                ? $"Order {orderNumber} is now {status}. Tracking: {trackingNumber}."
                : $"Order {orderNumber} is now {status}."
        };

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await _hubContext.Clients.Group(NotificationHub.UserGroup(userId)).SendAsync("notification", payload, cancellationToken);
        }

        await _hubContext.Clients.Group(NotificationHub.AdminGroup).SendAsync("notification", payload, cancellationToken);
    }
}