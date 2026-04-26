using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Admin;

namespace ASPNET_Ecommerce.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);

    Task<CategoryFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default);

    Task UpdateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}