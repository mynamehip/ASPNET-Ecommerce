using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Settings;

namespace ASPNET_Ecommerce.Models.ViewModels.Home;

public class HomeViewModel
{
    public IReadOnlyList<Category> Categories { get; set; } = [];
    public IReadOnlyList<Product> NewProducts { get; set; } = [];
    public IReadOnlyList<Product> FeaturedProducts { get; set; } = [];
    public IReadOnlyList<Product> DiscountProducts { get; set; } = [];
    public StorefrontSettingsViewModel Settings { get; set; } = new();
}
