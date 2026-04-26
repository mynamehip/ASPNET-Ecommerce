using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Services;

public class PaymentGatewayRequest
{
    public string OrderNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;
}