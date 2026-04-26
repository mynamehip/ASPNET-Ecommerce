using System.Text;
using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.ViewModels.Auth;
using ASPNET_Ecommerce.Models.ViewModels.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace ASPNET_Ecommerce.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<ProfilePageViewModel> GetProfilePageAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User account not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new ProfilePageViewModel
        {
            ProfileForm = new ProfileEditViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                DefaultAddress = user.DefaultAddress,
                DefaultWard = user.DefaultWard,
                DefaultProvince = user.DefaultProvince,
            },
            Roles = roles.ToList(),
            CreatedAtUtc = user.CreatedAtUtc,
            EmailConfirmed = user.EmailConfirmed
        };
    }

    public async Task UpdateProfileAsync(string userId, ProfileEditViewModel model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User account not found.");
        }

        var normalizedEmail = model.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null && !string.Equals(existingUser.Id, user.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        user.FullName = model.FullName.Trim();
        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;
        user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        user.DefaultAddress = string.IsNullOrWhiteSpace(model.DefaultAddress) ? null : model.DefaultAddress.Trim();
        user.DefaultWard = string.IsNullOrWhiteSpace(model.DefaultWard) ? null : model.DefaultWard.Trim();
        user.DefaultProvince = string.IsNullOrWhiteSpace(model.DefaultProvince) ? null : model.DefaultProvince.Trim();
        user.EmailConfirmed = true;

        var result = await _userManager.UpdateAsync(user);
        EnsureSuccess(result, "Unable to update your profile.");

        await _signInManager.RefreshSignInAsync(user);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordViewModel model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User account not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        EnsureSuccess(result, "Unable to change your password.");

        await _signInManager.RefreshSignInAsync(user);
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return null;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public async Task ResetPasswordAsync(ResetPasswordViewModel model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            throw new InvalidOperationException("Unable to reset the password with the provided information.");
        }

        string decodedToken;

        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("The password reset link is invalid or has expired.");
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
        EnsureSuccess(result, "Unable to reset the password.");
    }

    private static void EnsureSuccess(IdentityResult result, string fallbackMessage)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = result.Errors.Select(error => error.Description).ToArray();
        throw new InvalidOperationException(errors.Length > 0 ? string.Join(" ", errors) : fallbackMessage);
    }
}