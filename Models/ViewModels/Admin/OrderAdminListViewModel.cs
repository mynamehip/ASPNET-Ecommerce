using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class OrderAdminListViewModel
{
    public string? Search { get; set; }

    public OrderStatus? Status { get; set; }

    public IReadOnlyList<OrderAdminListItemViewModel> Orders { get; set; } = [];
}