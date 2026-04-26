namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class ProductImageViewModel
{
    public int Id { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}