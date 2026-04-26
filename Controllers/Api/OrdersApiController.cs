using System.Security.Claims;
using ASPNET_Ecommerce.Models.Api;
using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET_Ecommerce.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersApiController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var orders = await _orderService.GetHistoryAsync(userId, cancellationToken);
        return Ok(orders.Select(item => new
        {
            item.OrderId,
            item.OrderNumber,
            status = item.Status.ToString(),
            paymentStatus = item.PaymentStatus.ToString(),
            item.TrackingNumber,
            item.CreatedAtUtc,
            item.ItemCount,
            item.Subtotal
        }));
    }

    [HttpGet("{id:int}/tracking")]
    public async Task<ActionResult<OrderTrackingDto>> GetTracking(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var tracking = await _orderService.GetTrackingAsync(id, userId, cancellationToken);
        if (tracking is null)
        {
            return NotFound();
        }

        return Ok(new OrderTrackingDto
        {
            OrderId = tracking.OrderId,
            OrderNumber = tracking.OrderNumber,
            Status = tracking.Status.ToString(),
            PaymentMethod = tracking.PaymentMethod.ToString(),
            PaymentStatus = tracking.PaymentStatus.ToString(),
            PaymentProvider = tracking.PaymentProvider,
            PaymentTransactionReference = tracking.PaymentTransactionReference,
            ShippingFullName = tracking.ShippingFullName,
            ShippingWard = tracking.ShippingWard,
            ShippingProvince = tracking.ShippingProvince,
            ShipmentCarrier = tracking.ShipmentCarrier,
            TrackingNumber = tracking.TrackingNumber,
            TrackingUrl = tracking.TrackingUrl,
            CreatedAtUtc = tracking.CreatedAtUtc,
            EstimatedDeliveryDateUtc = tracking.EstimatedDeliveryDateUtc,
            ShippedAtUtc = tracking.ShippedAtUtc,
            DeliveredAtUtc = tracking.DeliveredAtUtc
        });
    }
}