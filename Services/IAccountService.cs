using ASPNET_Ecommerce.Models.ViewModels.Auth;
using ASPNET_Ecommerce.Models.ViewModels.Profile;

namespace ASPNET_Ecommerce.Services;

public interface IAccountService
{
    Task<ProfilePageViewModel> GetProfilePageAsync(string userId, CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(string userId, ProfileEditViewModel model, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(string userId, ChangePasswordViewModel model, CancellationToken cancellationToken = default);

    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(ResetPasswordViewModel model, CancellationToken cancellationToken = default);
}