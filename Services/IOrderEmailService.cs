using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Services;

public interface IOrderEmailService
{
    Task TrySendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default);
}