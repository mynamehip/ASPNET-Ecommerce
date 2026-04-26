using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Services;

public class PaymentGatewayResult
{
    public bool Succeeded { get; set; }

    public PaymentStatus Status { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string? TransactionReference { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? AuthorizedAtUtc { get; set; }

    public DateTime? PaidAtUtc { get; set; }
}