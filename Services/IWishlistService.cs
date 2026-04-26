using ASPNET_Ecommerce.Models.ViewModels.Wishlist;

namespace ASPNET_Ecommerce.Services;

public interface IWishlistService
{
    Task<WishlistIndexViewModel> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<int> GetItemCountAsync(CancellationToken cancellationToken = default);

    Task<bool> ContainsAsync(int productId, CancellationToken cancellationToken = default);

    Task AddAsync(int productId, CancellationToken cancellationToken = default);

    Task RemoveAsync(int productId, CancellationToken cancellationToken = default);
}