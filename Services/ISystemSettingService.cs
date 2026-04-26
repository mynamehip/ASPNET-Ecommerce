using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Models.ViewModels.Settings;

namespace ASPNET_Ecommerce.Services;

public interface ISystemSettingService
{
    Task EnsureDefaultAsync(CancellationToken cancellationToken = default);

    Task<StorefrontSettingsViewModel> GetPublicAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SliderItemViewModel>> GetActiveSliderItemsAsync(CancellationToken cancellationToken = default);

    Task<SettingFormViewModel> GetForEditAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(SettingFormViewModel model, CancellationToken cancellationToken = default);
}
