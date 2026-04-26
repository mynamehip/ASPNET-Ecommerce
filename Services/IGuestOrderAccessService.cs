namespace ASPNET_Ecommerce.Services;

public interface IGuestOrderAccessService
{
    void GrantAccess(int orderId);

    bool HasAccess(int orderId);
}