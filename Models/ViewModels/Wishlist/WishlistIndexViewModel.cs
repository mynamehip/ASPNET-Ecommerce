namespace ASPNET_Ecommerce.Models.ViewModels.Wishlist;

public class WishlistIndexViewModel
{
    public IReadOnlyList<WishlistItemViewModel> Items { get; set; } = [];

    public int ItemCount => Items.Count;
}