using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Admin;

namespace ASPNET_Ecommerce.Services;

public interface IProductService
{
    Task<IReadOnlyList<AdminProductListItemViewModel>> GetAdminListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 8, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetNewArrivalsAsync(int count = 8, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetDiscountedAsync(int count = 8, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetActiveAsync(
        string? search = null,
        int? categoryId = null,
        string? sortBy = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool inStockOnly = false,
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetRelatedAsync(int productId, int categoryId, int count = 4, CancellationToken cancellationToken = default);

    Task<ProductFormViewModel> BuildCreateModelAsync(CancellationToken cancellationToken = default);

    Task<ProductFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    Task PopulateLookupsAsync(ProductFormViewModel model, CancellationToken cancellationToken = default);

    Task CreateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default);

    Task UpdateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
