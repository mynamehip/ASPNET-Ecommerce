namespace ASPNET_Ecommerce.Models.ViewModels.Cart;

public class CartIndexViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];

    public int ItemCount { get; set; }

    public decimal Subtotal { get; set; }
}