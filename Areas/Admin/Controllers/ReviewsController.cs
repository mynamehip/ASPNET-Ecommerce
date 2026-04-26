using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ASPNET_Ecommerce.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ReviewsController(IReviewService reviewService, IStringLocalizer<SharedResource> localizer)
    {
        _reviewService = reviewService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, ProductReviewStatus? status, CancellationToken cancellationToken)
    {
        var model = await _reviewService.GetAdminListAsync(search, status, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await _reviewService.GetAdminDetailsAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, [Bind(Prefix = "StatusForm")] ReviewStatusUpdateViewModel statusForm, CancellationToken cancellationToken)
    {
        if (id != statusForm.ReviewId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await _reviewService.GetAdminDetailsAsync(id, cancellationToken);
            if (invalidModel is null)
            {
                return NotFound();
            }

            invalidModel.StatusForm = statusForm;
            return View("Details", invalidModel);
        }

        try
        {
            await _reviewService.UpdateStatusAsync(statusForm.ReviewId, statusForm, cancellationToken);
            TempData["StatusMessage"] = _localizer["Message.Admin.ReviewUpdated"].Value;
            return RedirectToAction(nameof(Details), new { id = statusForm.ReviewId });
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Details), new { id = statusForm.ReviewId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _reviewService.DeleteAsync(id, cancellationToken);
            TempData["StatusMessage"] = _localizer["Message.Admin.ReviewDeleted"].Value;
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}