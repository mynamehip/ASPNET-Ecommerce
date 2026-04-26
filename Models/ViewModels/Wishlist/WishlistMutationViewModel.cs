using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.ViewModels.Wishlist;

public class WishlistMutationViewModel
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    public string? ReturnUrl { get; set; }
}