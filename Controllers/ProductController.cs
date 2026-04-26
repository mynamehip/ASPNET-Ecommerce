using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Products;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace ASPNET_Ecommerce.Controllers;

public class ProductController : Controller
{
    private const int PageSize = 12;

    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IReviewService _reviewService;
    private readonly IWishlistService _wishlistService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ProductController(IProductService productService, ICategoryService categoryService, IReviewService reviewService, IWishlistService wishlistService, IStringLocalizer<SharedResource> localizer)
    {
        _productService = productService;
        _categoryService = categoryService;
        _reviewService = reviewService;
        _wishlistService = wishlistService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, string? sortBy, decimal? minPrice, decimal? maxPrice, bool inStockOnly = false, int page = 1)
    {
        if (page < 1) page = 1;

        var categories = await _categoryService.GetActiveCategoriesAsync();
        var (items, totalCount) = await _productService.GetActiveAsync(search, categoryId, sortBy, minPrice, maxPrice, inStockOnly, page, PageSize);
        var activeProducts = await _productService.GetActiveAsync(page: 1, pageSize: int.MaxValue);
        var priceRangeMin = activeProducts.Items.Count > 0 ? activeProducts.Items.Min(item => item.EffectivePrice) : 0;
        var priceRangeMax = activeProducts.Items.Count > 0 ? activeProducts.Items.Max(item => item.EffectivePrice) : 0;

        var model = new ProductListViewModel
        {
            Products = items,
            Categories = categories,
            SearchQuery = search,
            CategoryId = categoryId,
            SortBy = sortBy,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            InStockOnly = inStockOnly,
            PriceRangeMin = priceRangeMin,
            PriceRangeMax = priceRangeMax,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize),
            TotalItems = totalCount
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var relatedProducts = await _productService.GetRelatedAsync(product.Id, product.CategoryId);
        var reviews = await _reviewService.GetByProductAsync(product.Id);
        var (avgRating, reviewCount) = await _reviewService.GetStatsAsync(product.Id);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userHasReviewed = false;
        var isInWishlist = await _wishlistService.ContainsAsync(product.Id);
        ProductReviewStatus? currentUserReviewStatus = null;
        var canCurrentUserReview = false;
        string? reviewEligibilityMessage = null;
        if (userId is not null)
        {
            var existing = await _reviewService.GetUserReviewAsync(product.Id, userId);
            userHasReviewed = existing is not null;
            currentUserReviewStatus = existing?.Status;

            var eligibility = await _reviewService.GetReviewEligibilityAsync(product.Id, userId);
            canCurrentUserReview = eligibility.CanReview;
            reviewEligibilityMessage = eligibility.Message;
        }

        var model = new ProductDetailViewModel
        {
            Product = product,
            RelatedProducts = relatedProducts,
            Reviews = reviews,
            IsInWishlist = isInWishlist,
            AverageRating = avgRating,
            ReviewCount = reviewCount,
            UserHasReviewed = userHasReviewed,
            CanCurrentUserReview = canCurrentUserReview,
            ReviewEligibilityMessage = reviewEligibilityMessage,
            CurrentUserReviewStatus = currentUserReviewStatus,
            ReviewForm = new ReviewFormViewModel { ProductId = product.Id }
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview([Bind(Prefix = "ReviewForm")] ReviewFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ReviewError"] = _localizer["Message.Review.InvalidRating"].Value;
            return RedirectToAction(nameof(Details), new { id = model.ProductId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        try
        {
            await _reviewService.CreateAsync(userId, model);
            TempData["ReviewSuccess"] = _localizer["Message.Review.Thanks"].Value;
        }
        catch (InvalidOperationException ex)
        {
            TempData["ReviewError"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.ProductId });
    }
}
