using ASPNET_Ecommerce.Models.Orders;

namespace ASPNET_Ecommerce.Services;

public class DemoPaymentGatewayService : IPaymentGatewayService
{
    public Task<PaymentGatewayResult> ProcessAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.Amount <= 0)
        {
            return Task.FromResult(new PaymentGatewayResult
            {
                Succeeded = false,
                Status = PaymentStatus.Failed,
                Provider = "Validation",
                FailureReason = "Payment amount must be greater than zero."
            });
        }

        return Task.FromResult(request.Method switch
        {
            PaymentMethod.CashOnDelivery => new PaymentGatewayResult
            {
                Succeeded = true,
                Status = PaymentStatus.Pending,
                Provider = "Cash on Delivery"
            },
            PaymentMethod.DemoGateway => new PaymentGatewayResult
            {
                Succeeded = true,
                Status = PaymentStatus.Paid,
                Provider = "DemoPay",
                TransactionReference = $"DMP-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
                AuthorizedAtUtc = DateTime.UtcNow,
                PaidAtUtc = DateTime.UtcNow
            },
            _ => new PaymentGatewayResult
            {
                Succeeded = false,
                Status = PaymentStatus.Failed,
                Provider = "Unknown",
                FailureReason = "Unsupported payment method."
            }
        });
    }
}