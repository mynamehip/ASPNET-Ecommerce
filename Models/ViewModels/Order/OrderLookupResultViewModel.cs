namespace ASPNET_Ecommerce.Models.ViewModels.Order;

public class OrderLookupResultViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public bool IsGuestOrder { get; set; }
}