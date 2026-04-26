using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Models.ViewModels.Products;

namespace ASPNET_Ecommerce.Services;

public interface IReviewService
{
    Task<IReadOnlyList<ProductReview>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);

    Task<ProductReview?> GetUserReviewAsync(int productId, string userId, CancellationToken cancellationToken = default);

    Task<(bool CanReview, string? Message)> GetReviewEligibilityAsync(int productId, string userId, CancellationToken cancellationToken = default);

    Task CreateAsync(string userId, ReviewFormViewModel model, CancellationToken cancellationToken = default);

    Task<(double AverageRating, int TotalCount)> GetStatsAsync(int productId, CancellationToken cancellationToken = default);

    Task<ReviewAdminListViewModel> GetAdminListAsync(string? search, ProductReviewStatus? status, CancellationToken cancellationToken = default);

    Task<ReviewAdminDetailsViewModel?> GetAdminDetailsAsync(int reviewId, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(int reviewId, ReviewStatusUpdateViewModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int reviewId, CancellationToken cancellationToken = default);
}
