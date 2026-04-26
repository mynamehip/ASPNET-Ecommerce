using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Models.ViewModels.Products;

public class ProductListViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; set; } = [];

    public string? SearchQuery { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStockOnly { get; set; }
    public decimal PriceRangeMin { get; set; }
    public decimal PriceRangeMax { get; set; }

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}
