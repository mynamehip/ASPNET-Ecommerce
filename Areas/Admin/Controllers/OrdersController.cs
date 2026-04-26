using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace ASPNET_Ecommerce.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public OrdersController(IOrderService orderService, IStringLocalizer<SharedResource> localizer)
    {
        _orderService = orderService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, ASPNET_Ecommerce.Models.Orders.OrderStatus? status, CancellationToken cancellationToken)
    {
        var model = await _orderService.GetAdminOrderListAsync(search, status, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await _orderService.GetAdminDetailsAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, [Bind(Prefix = "StatusForm")] OrderStatusUpdateViewModel statusForm, CancellationToken cancellationToken)
    {
        if (id != statusForm.OrderId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await _orderService.GetAdminDetailsAsync(id, cancellationToken);
            if (invalidModel is null)
            {
                return NotFound();
            }

            invalidModel.StatusForm = statusForm;
            return View("Details", invalidModel);
        }

        try
        {
            await _orderService.UpdateStatusAsync(
                statusForm.OrderId,
                statusForm,
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                User.Identity?.Name ?? "Admin",
                cancellationToken);
            TempData["StatusMessage"] = _localizer["Message.Admin.OrderUpdated"].Value;
            return RedirectToAction(nameof(Details), new { id = statusForm.OrderId });
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Details), new { id = statusForm.OrderId });
        }
    }
}