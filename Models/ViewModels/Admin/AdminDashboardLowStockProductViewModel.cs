using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class AdminDashboardLowStockProductViewModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public ProductStatus Status { get; set; }

    public DateTime ReferenceAtUtc { get; set; }
}