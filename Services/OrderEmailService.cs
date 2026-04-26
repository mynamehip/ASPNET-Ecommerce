using System.Net;
using System.Net.Mail;
using System.Text;
using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class OrderEmailService : IOrderEmailService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger<OrderEmailService> _logger;

    public OrderEmailService(
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator,
        ILogger<OrderEmailService> logger)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
        _logger = logger;
    }

    public async Task TrySendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default)
    {
        var settings = await _dbContext.SystemSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            _logger.LogInformation("Skipping confirmation email for order {OrderNumber} because system settings are not initialized.", order.OrderNumber);
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.SmtpHost) || string.IsNullOrWhiteSpace(settings.SmtpFromEmail))
        {
            _logger.LogInformation("Skipping confirmation email for order {OrderNumber} because SMTP settings are incomplete.", order.OrderNumber);
            return;
        }

        if (string.IsNullOrWhiteSpace(order.ShippingEmail))
        {
            _logger.LogInformation("Skipping confirmation email for order {OrderNumber} because the customer email is empty.", order.OrderNumber);
            return;
        }

        if (!string.IsNullOrWhiteSpace(settings.SmtpUsername) && string.IsNullOrWhiteSpace(settings.SmtpPassword))
        {
            _logger.LogInformation("Skipping confirmation email for order {OrderNumber} because SMTP username is configured without a password.", order.OrderNumber);
            return;
        }

        using var message = BuildMessage(settings.StoreName, settings.SupportEmail, settings.SmtpFromEmail, order);
        using var client = BuildClient(settings.SmtpHost, settings.SmtpPort, settings.SmtpUsername, settings.SmtpPassword);

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to send confirmation email for order {OrderNumber}.", order.OrderNumber);
        }
    }

    private MailMessage BuildMessage(string storeName, string supportEmail, string fromEmail, Order order)
    {
        var lookupUrl = BuildLookupUrl(order.OrderNumber);
        var body = new StringBuilder()
            .AppendLine($"Hello {order.ShippingFullName},")
            .AppendLine()
            .AppendLine($"Thank you for your order at {storeName}.")
            .AppendLine($"Order number: {order.OrderNumber}")
            .AppendLine($"Placed at: {order.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC")
            .AppendLine($"Total: {order.TotalAmount:N0} {order.CurrencyCode}")
            .AppendLine($"Payment status: {order.PaymentStatus}")
            .AppendLine()
            .AppendLine("Items:");

        foreach (var item in order.Items)
        {
            body.AppendLine($"- {item.ProductName} x {item.Quantity} ({item.UnitPrice:N0} {order.CurrencyCode})");
        }

        body.AppendLine()
            .AppendLine("To look this order up later, use the order number above on the order lookup page.");

        if (!string.IsNullOrWhiteSpace(lookupUrl))
        {
            body.AppendLine(lookupUrl);
        }

        if (!string.IsNullOrWhiteSpace(supportEmail))
        {
            body.AppendLine()
                .AppendLine($"Support: {supportEmail}");
        }

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, storeName),
            Subject = $"[{storeName}] Order {order.OrderNumber} confirmed",
            Body = body.ToString(),
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8,
            IsBodyHtml = false
        };

        message.To.Add(new MailAddress(order.ShippingEmail, order.ShippingFullName));
        if (!string.IsNullOrWhiteSpace(supportEmail))
        {
            message.ReplyToList.Add(new MailAddress(supportEmail));
        }

        return message;
    }

    private SmtpClient BuildClient(string smtpHost, int? smtpPort, string? smtpUsername, string? smtpPassword)
    {
        var port = smtpPort ?? 25;
        var client = new SmtpClient(smtpHost, port)
        {
            EnableSsl = port is 465 or 587,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(smtpUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
        {
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
        }

        return client;
    }

    private string? BuildLookupUrl(string orderNumber)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        return _linkGenerator.GetUriByAction(
            httpContext,
            action: "Lookup",
            controller: "Order",
            values: new { orderNumber });
    }
}