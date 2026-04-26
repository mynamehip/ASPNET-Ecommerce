using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ASPNET_Ecommerce.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.Employee}")]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ProductsController(IProductService productService, IStringLocalizer<SharedResource> localizer)
    {
        _productService = productService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAdminListAsync(cancellationToken);
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _productService.BuildCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await _productService.PopulateLookupsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            await _productService.CreateAsync(model, cancellationToken);
            TempData["StatusMessage"] = _localizer["Message.Admin.ProductCreated"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await _productService.PopulateLookupsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _productService.GetForEditAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await _productService.PopulateLookupsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            await _productService.UpdateAsync(model, cancellationToken);
            TempData["StatusMessage"] = _localizer["Message.Admin.ProductUpdated"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await _productService.PopulateLookupsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteAsync(id, cancellationToken);
            TempData["StatusMessage"] = _localizer["Message.Admin.ProductDeleted"].Value;
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
