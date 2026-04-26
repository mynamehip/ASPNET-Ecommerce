using ASPNET_Ecommerce.Models.ViewModels.Order;
using ASPNET_Ecommerce.Models.Orders;
using ASPNET_Ecommerce.Models.ViewModels.Admin;

namespace ASPNET_Ecommerce.Services;

public interface IOrderService
{
    Task<CheckoutViewModel> BuildCheckoutAsync(string? userId, CancellationToken cancellationToken = default);

    Task PopulateCheckoutAsync(CheckoutViewModel model, CancellationToken cancellationToken = default);

    Task<int> CreateOrderAsync(string? userId, CheckoutViewModel model, CancellationToken cancellationToken = default);

    Task<OrderConfirmationViewModel?> GetConfirmationAsync(int orderId, string? userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderHistoryItemViewModel>> GetHistoryAsync(string userId, CancellationToken cancellationToken = default);

    Task<OrderDetailsViewModel?> GetDetailsAsync(int orderId, string? userId, CancellationToken cancellationToken = default);

    Task<OrderTrackingViewModel?> GetTrackingAsync(int orderId, string? userId, CancellationToken cancellationToken = default);

    Task<OrderLookupResultViewModel?> LookupAsync(string orderNumber, string? userId, string? email, CancellationToken cancellationToken = default);

    Task<OrderAdminListViewModel> GetAdminOrderListAsync(string? search, OrderStatus? status, CancellationToken cancellationToken = default);

    Task<OrderAdminDetailsViewModel?> GetAdminDetailsAsync(int orderId, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(int orderId, OrderStatusUpdateViewModel model, string? actorUserId, string actorDisplayName, CancellationToken cancellationToken = default);

    Task CancelAsync(int orderId, string userId, string actorDisplayName, string? reason, CancellationToken cancellationToken = default);
}