using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.ViewModels.Cart;

public class UpdateCartItemViewModel
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999.")]
    public int Quantity { get; set; }
}