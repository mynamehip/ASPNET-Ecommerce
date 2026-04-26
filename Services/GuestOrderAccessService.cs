using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ASPNET_Ecommerce.Services;

public class GuestOrderAccessService : IGuestOrderAccessService
{
    private const string SessionKey = "guest-order.access";
    private const int MaxTrackedOrders = 20;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public GuestOrderAccessService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void GrantAccess(int orderId)
    {
        var orderIds = GetOrderIds();
        orderIds.Remove(orderId);
        orderIds.Insert(0, orderId);

        if (orderIds.Count > MaxTrackedOrders)
        {
            orderIds = orderIds.Take(MaxTrackedOrders).ToList();
        }

        SaveOrderIds(orderIds);
    }

    public bool HasAccess(int orderId)
    {
        return GetOrderIds().Contains(orderId);
    }

    private List<int> GetOrderIds()
    {
        var json = GetSession().GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<int>>(json) ?? [];
    }

    private void SaveOrderIds(List<int> orderIds)
    {
        var session = GetSession();
        if (orderIds.Count == 0)
        {
            session.Remove(SessionKey);
            return;
        }

        session.SetString(SessionKey, JsonSerializer.Serialize(orderIds));
    }

    private ISession GetSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            throw new InvalidOperationException("Session is not available for the current request.");
        }

        return session;
    }
}