using ASPNET_Ecommerce.Models.Settings;
using ASPNET_Ecommerce.Models.ViewModels.Admin;

namespace ASPNET_Ecommerce.Services;

public interface ISliderService
{
    Task<SliderIndexViewModel> GetAdminIndexAsync(CancellationToken cancellationToken = default);

    Task<SliderFormViewModel> BuildCreateModelAsync(SliderItemType itemType, CancellationToken cancellationToken = default);

    Task<SliderFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(SliderFormViewModel model, CancellationToken cancellationToken = default);

    Task UpdateAsync(SliderFormViewModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
