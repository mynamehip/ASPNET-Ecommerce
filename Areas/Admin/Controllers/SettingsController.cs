using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ASPNET_Ecommerce.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class SettingsController : Controller
{
    private readonly ISystemSettingService _systemSettingService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SettingsController(ISystemSettingService systemSettingService, IStringLocalizer<SharedResource> localizer)
    {
        _systemSettingService = systemSettingService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _systemSettingService.GetForEditAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _systemSettingService.UpdateAsync(model, cancellationToken);
            TempData["StatusMessage"] = _localizer["Message.Admin.SettingsUpdated"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }
}