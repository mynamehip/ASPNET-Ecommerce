using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ASPNET_Ecommerce.Models;
using ASPNET_Ecommerce.Models.ViewModels.Home;
using ASPNET_Ecommerce.Services;

namespace ASPNET_Ecommerce.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ISystemSettingService _systemSettingService;

    public HomeController(IProductService productService, ICategoryService categoryService, ISystemSettingService systemSettingService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _systemSettingService = systemSettingService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _systemSettingService.GetPublicAsync();
        var model = new HomeViewModel
        {
            Categories = settings.ShowHomepageCategories
                ? await _categoryService.GetActiveCategoriesAsync()
                : [],
            NewProducts = settings.ShowHomepageNewProducts
                ? await _productService.GetNewArrivalsAsync(8)
                : [],
            FeaturedProducts = settings.ShowHomepageFeaturedProducts
                ? await _productService.GetFeaturedAsync(8)
                : [],
            DiscountProducts = settings.ShowHomepageDiscountProducts
                ? await _productService.GetDiscountedAsync(8)
                : [],
            Settings = settings
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
