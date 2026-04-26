namespace ASPNET_Ecommerce.Models.ViewModels.Cart;

public class CartItemViewModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? ProductSku { get; set; }

    public string? ImagePath { get; set; }

    public decimal OriginalUnitPrice { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsDiscountActive { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public int Quantity { get; set; }

    public int AvailableStock { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
