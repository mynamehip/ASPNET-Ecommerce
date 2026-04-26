namespace ASPNET_Ecommerce.Services;

public interface IPaymentGatewayService
{
    Task<PaymentGatewayResult> ProcessAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default);
}