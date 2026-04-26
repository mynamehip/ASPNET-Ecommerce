namespace ASPNET_Ecommerce.Models.Api;

public class ProductSummaryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Sku { get; set; }

    public decimal Price { get; set; }

    public decimal EffectivePrice { get; set; }

    public bool IsDiscountActive { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public int StockQuantity { get; set; }

    public string Status { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? PrimaryImagePath { get; set; }
}
