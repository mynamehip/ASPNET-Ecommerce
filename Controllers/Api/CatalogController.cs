using ASPNET_Ecommerce.Models.Api;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET_Ecommerce.Controllers.Api;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly IReviewService _reviewService;

    public CatalogController(ICategoryService categoryService, IProductService productService, IReviewService reviewService)
    {
        _categoryService = categoryService;
        _productService = productService;
        _reviewService = reviewService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategorySummaryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetActiveCategoriesAsync(cancellationToken);
        return Ok(categories
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new CategorySummaryDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                DisplayOrder = item.DisplayOrder
            })
            .ToList());
    }

    [HttpGet("products")]
    public async Task<ActionResult<object>> GetProducts([FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] string? sortBy, [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, totalCount) = await _productService.GetActiveAsync(
            search,
            categoryId,
            sortBy,
            page: page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            items = items.Select(MapProductSummary).ToList()
        });
    }

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var relatedProducts = await _productService.GetRelatedAsync(product.Id, product.CategoryId, cancellationToken: cancellationToken);
        var (averageRating, reviewCount) = await _reviewService.GetStatsAsync(product.Id, cancellationToken);

        return Ok(new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            Price = product.Price,
            EffectivePrice = product.EffectivePrice,
            IsDiscountActive = product.HasActiveDiscount,
            DiscountPercentage = product.DiscountPercentage,
            StockQuantity = product.StockQuantity,
            Status = product.Status.ToString(),
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            PrimaryImagePath = product.Images.OrderBy(image => image.DisplayOrder).FirstOrDefault(image => image.IsPrimary)?.ImagePath
                ?? product.Images.OrderBy(image => image.DisplayOrder).FirstOrDefault()?.ImagePath,
            AverageRating = averageRating,
            ReviewCount = reviewCount,
            ImagePaths = product.Images.OrderBy(image => image.DisplayOrder).Select(image => image.ImagePath).ToList(),
            RelatedProducts = relatedProducts.Select(MapProductSummary).ToList()
        });
    }

    private static ProductSummaryDto MapProductSummary(ASPNET_Ecommerce.Models.Catalog.Product product)
    {
        return new ProductSummaryDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Price = product.Price,
            EffectivePrice = product.EffectivePrice,
            IsDiscountActive = product.HasActiveDiscount,
            DiscountPercentage = product.DiscountPercentage,
            StockQuantity = product.StockQuantity,
            Status = product.Status.ToString(),
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            PrimaryImagePath = product.Images.OrderBy(image => image.DisplayOrder).FirstOrDefault(image => image.IsPrimary)?.ImagePath
                ?? product.Images.OrderBy(image => image.DisplayOrder).FirstOrDefault()?.ImagePath
        };
    }
}
