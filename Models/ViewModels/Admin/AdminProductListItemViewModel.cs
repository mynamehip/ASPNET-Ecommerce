using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class AdminProductListItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Sku { get; set; }

    public string? CategoryName { get; set; }

    public decimal OriginalPrice { get; set; }

    public decimal EffectivePrice { get; set; }

    public bool IsDiscountActive { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public int StockQuantity { get; set; }

    public ProductStatus Status { get; set; }

    public int ImageCount { get; set; }

    public string? PrimaryImagePath { get; set; }
}
