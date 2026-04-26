using System.Security.Claims;
using ASPNET_Ecommerce.Models.ViewModels.Order;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ASPNET_Ecommerce.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IGuestOrderAccessService _guestOrderAccessService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public OrderController(IOrderService orderService, IGuestOrderAccessService guestOrderAccessService, IStringLocalizer<SharedResource> localizer)
    {
        _orderService = orderService;
        _guestOrderAccessService = guestOrderAccessService;
        _localizer = localizer;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var orders = await _orderService.GetHistoryAsync(userId, cancellationToken);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var model = await _orderService.BuildCheckoutAsync(userId, cancellationToken);
            if (!model.HasItems)
            {
                TempData["CartError"] = _localizer["Message.Cart.Empty"].Value;
                return RedirectToAction("Index", "Cart");
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["CartError"] = ex.Message;
            return RedirectToAction("Index", "Cart");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            model.SaveAsDefaultAddress = false;
        }

        await _orderService.PopulateCheckoutAsync(model, cancellationToken);
        if (!model.HasItems)
        {
            TempData["CartError"] = _localizer["Message.Cart.Empty"].Value;
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var orderId = await _orderService.CreateOrderAsync(userId, model, cancellationToken);
            if (userId is null)
            {
                _guestOrderAccessService.GrantAccess(orderId);
            }

            TempData["OrderSuccess"] = _localizer["Message.Order.Placed"].Value;
            return RedirectToAction(nameof(Confirmation), new { id = orderId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["OrderError"] = ex.Message;
            await _orderService.PopulateCheckoutAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id, CancellationToken cancellationToken)
    {
        var model = await GetAccessibleConfirmationAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await GetAccessibleDetailsAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Tracking(int id, CancellationToken cancellationToken)
    {
        var model = await GetAccessibleTrackingAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Lookup(string? orderNumber = null, string? email = null)
    {
        return View(new OrderLookupViewModel
        {
            OrderNumber = orderNumber?.Trim() ?? string.Empty,
            Email = email?.Trim()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lookup(OrderLookupViewModel model, CancellationToken cancellationToken)
    {
        if (!(User.Identity?.IsAuthenticated ?? false) && string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email is required to look up guest orders.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _orderService.LookupAsync(model.OrderNumber, GetCurrentUserId(), model.Email, cancellationToken);
        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "We could not find an accessible order with that code.");
            return View(model);
        }

        if (result.IsGuestOrder)
        {
            _guestOrderAccessService.GrantAccess(result.OrderId);
        }

        return RedirectToAction(nameof(Details), new { id = result.OrderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Cancel(int id, string? reason, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Challenge();
        }

        try
        {
            await _orderService.CancelAsync(id, userId, User.Identity?.Name ?? "Customer", reason, cancellationToken);
            TempData["OrderSuccess"] = _localizer["Message.Order.Cancelled"].Value;
        }
        catch (InvalidOperationException ex)
        {
            TempData["OrderError"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<OrderConfirmationViewModel?> GetAccessibleConfirmationAsync(int orderId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var ownedOrder = await _orderService.GetConfirmationAsync(orderId, userId, cancellationToken);
            if (ownedOrder is not null)
            {
                return ownedOrder;
            }
        }

        if (_guestOrderAccessService.HasAccess(orderId))
        {
            return await _orderService.GetConfirmationAsync(orderId, null, cancellationToken);
        }

        return null;
    }

    private async Task<OrderDetailsViewModel?> GetAccessibleDetailsAsync(int orderId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var ownedOrder = await _orderService.GetDetailsAsync(orderId, userId, cancellationToken);
            if (ownedOrder is not null)
            {
                return ownedOrder;
            }
        }

        if (_guestOrderAccessService.HasAccess(orderId))
        {
            return await _orderService.GetDetailsAsync(orderId, null, cancellationToken);
        }

        return null;
    }

    private async Task<OrderTrackingViewModel?> GetAccessibleTrackingAsync(int orderId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var ownedOrder = await _orderService.GetTrackingAsync(orderId, userId, cancellationToken);
            if (ownedOrder is not null)
            {
                return ownedOrder;
            }
        }

        if (_guestOrderAccessService.HasAccess(orderId))
        {
            return await _orderService.GetTrackingAsync(orderId, null, cancellationToken);
        }

        return null;
    }
}