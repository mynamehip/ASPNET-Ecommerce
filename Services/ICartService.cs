using ASPNET_Ecommerce.Models.ViewModels.Cart;

namespace ASPNET_Ecommerce.Services;

public interface ICartService
{
    Task<CartIndexViewModel> GetCartAsync(CancellationToken cancellationToken = default);

    Task<int> GetItemCountAsync();

    Task AddItemAsync(int productId, int quantity, CancellationToken cancellationToken = default);

    Task UpdateQuantityAsync(int productId, int quantity, CancellationToken cancellationToken = default);

    Task RemoveItemAsync(int productId);

    Task ClearAsync();
}