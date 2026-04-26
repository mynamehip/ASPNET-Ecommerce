using ASPNET_Ecommerce.Models.ViewModels.Cart;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ASPNET_Ecommerce.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CartController(ICartService cartService, IStringLocalizer<SharedResource> localizer)
    {
        _cartService = cartService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _cartService.GetCartAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["CartError"] = _localizer["Message.Cart.InvalidQuantity"].Value;
            return RedirectToReturnUrl(model.ReturnUrl, model.ProductId);
        }

        try
        {
            await _cartService.AddItemAsync(model.ProductId, model.Quantity, cancellationToken);
            TempData["CartSuccess"] = _localizer["Message.Cart.Added"].Value;
        }
        catch (InvalidOperationException ex)
        {
            TempData["CartError"] = ex.Message;
        }

        return RedirectToReturnUrl(model.ReturnUrl, model.ProductId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateCartItemViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["CartError"] = _localizer["Message.Cart.InvalidQuantity"].Value;
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _cartService.UpdateQuantityAsync(model.ProductId, model.Quantity, cancellationToken);
            TempData["CartSuccess"] = _localizer["Message.Cart.Updated"].Value;
        }
        catch (InvalidOperationException ex)
        {
            TempData["CartError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _cartService.RemoveItemAsync(productId);
        TempData["CartSuccess"] = _localizer["Message.Cart.Removed"].Value;
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToReturnUrl(string? returnUrl, int productId)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (productId > 0)
        {
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        return RedirectToAction(nameof(Index));
    }
}