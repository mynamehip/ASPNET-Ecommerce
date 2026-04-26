using System.Security.Claims;
using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ASPNET_Ecommerce.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class UsersController : Controller
{
    private readonly IUserAdminService _userAdminService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UsersController(IUserAdminService userAdminService, IStringLocalizer<SharedResource> localizer)
    {
        _userAdminService = userAdminService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? role, CancellationToken cancellationToken)
    {
        var model = await _userAdminService.GetListAsync(search, role, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Challenge();
        }

        var model = await _userAdminService.GetDetailsAsync(id, currentUserId, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string id, UserAdminUpdateViewModel model, CancellationToken cancellationToken)
    {
        if (!string.Equals(id, model.Id, StringComparison.Ordinal))
        {
            return BadRequest();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await _userAdminService.GetDetailsAsync(id, currentUserId, cancellationToken);
            if (invalidModel is null)
            {
                return NotFound();
            }

            invalidModel.UpdateForm = model;
            return View("Details", invalidModel);
        }

        try
        {
            await _userAdminService.UpdateAsync(currentUserId, model, cancellationToken);
            TempData["StatusMessage"] = _localizer["Message.Admin.UserUpdated"].Value;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (InvalidOperationException exception)
        {
            var invalidModel = await _userAdminService.GetDetailsAsync(id, currentUserId, cancellationToken);
            if (invalidModel is null)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, exception.Message);
            invalidModel.UpdateForm = model;
            return View("Details", invalidModel);
        }
    }
}