using ASPNET_Ecommerce.Models.ViewModels.Wishlist;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ASPNET_Ecommerce.Controllers;

public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public WishlistController(IWishlistService wishlistService, IStringLocalizer<SharedResource> localizer)
    {
        _wishlistService = wishlistService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _wishlistService.GetCurrentAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(WishlistMutationViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["WishlistError"] = _localizer["Message.Wishlist.SaveFailed"].Value;
            return RedirectToLocalOrFallback(model.ReturnUrl, model.ProductId);
        }

        try
        {
            await _wishlistService.AddAsync(model.ProductId, cancellationToken);
            TempData["WishlistStatusMessage"] = _localizer["Message.Wishlist.Saved"].Value;
        }
        catch (InvalidOperationException exception)
        {
            TempData["WishlistError"] = exception.Message;
        }

        return RedirectToLocalOrFallback(model.ReturnUrl, model.ProductId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(WishlistMutationViewModel model, CancellationToken cancellationToken)
    {
        await _wishlistService.RemoveAsync(model.ProductId, cancellationToken);
        TempData["WishlistStatusMessage"] = _localizer["Message.Wishlist.Removed"].Value;
        return RedirectToLocalOrFallback(model.ReturnUrl, model.ProductId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId, CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return BadRequest();
        }

        try
        {
            var isInWishlist = await _wishlistService.ContainsAsync(productId, cancellationToken);
            if (isInWishlist)
            {
                await _wishlistService.RemoveAsync(productId, cancellationToken);
            }
            else
            {
                await _wishlistService.AddAsync(productId, cancellationToken);
            }

            return Json(new
            {
                isInWishlist = !isInWishlist,
                itemCount = await _wishlistService.GetItemCountAsync(cancellationToken)
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private IActionResult RedirectToLocalOrFallback(string? returnUrl, int productId)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Details", "Product", new { id = productId });
    }
}