using System.Security.Claims;
using ASPNET_Ecommerce.Models.ViewModels.Profile;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET_Ecommerce.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IAccountService _accountService;

    public ProfileController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var model = await _accountService.GetProfilePageAsync(userId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileEditViewModel model, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var pageModel = await _accountService.GetProfilePageAsync(userId, cancellationToken);
            pageModel.ProfileForm = model;
            return View(pageModel);
        }

        try
        {
            await _accountService.UpdateProfileAsync(userId, model, cancellationToken);
            TempData["ProfileStatusMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            TempData["ProfileErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var pageModel = await _accountService.GetProfilePageAsync(userId, cancellationToken);
            pageModel.ChangePasswordForm = model;
            return View("Index", pageModel);
        }

        try
        {
            await _accountService.ChangePasswordAsync(userId, model, cancellationToken);
            TempData["PasswordStatusMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            TempData["PasswordErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}