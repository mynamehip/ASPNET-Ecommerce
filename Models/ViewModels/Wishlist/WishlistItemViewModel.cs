namespace ASPNET_Ecommerce.Models.ViewModels.Wishlist;

public class WishlistItemViewModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? ProductSku { get; set; }

    public string? CategoryName { get; set; }

    public string? ImagePath { get; set; }

    public decimal OriginalPrice { get; set; }

    public decimal EffectivePrice { get; set; }

    public bool IsDiscountActive { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public DateTime AddedAtUtc { get; set; }
}
