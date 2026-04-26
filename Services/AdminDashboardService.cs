using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.Orders;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminDashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var roleCounts = await (from userRole in _dbContext.UserRoles
                                join role in _dbContext.Roles on userRole.RoleId equals role.Id
                                group userRole by role.Name into grouped
                                select new
                                {
                                    Role = grouped.Key ?? string.Empty,
                                    Count = grouped.Count()
                                })
            .ToDictionaryAsync(item => item.Role, item => item.Count, cancellationToken);

        var recentOrders = await _dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAtUtc)
            .Take(5)
            .Select(order => new AdminDashboardRecentOrderViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.ShippingFullName,
                Status = order.Status,
                Subtotal = order.Subtotal,
                CreatedAtUtc = order.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var lowStockProducts = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Status == ProductStatus.Active && product.StockQuantity <= 5)
            .OrderBy(product => product.StockQuantity)
            .ThenByDescending(product => product.UpdatedAtUtc ?? product.CreatedAtUtc)
            .Take(6)
            .Select(product => new AdminDashboardLowStockProductViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                ReferenceAtUtc = product.UpdatedAtUtc ?? product.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new AdminDashboardViewModel
        {
            TotalUsers = await _dbContext.Users.AsNoTracking().CountAsync(cancellationToken),
            TotalCustomers = roleCounts.GetValueOrDefault(ApplicationRoles.User),
            TotalEmployees = roleCounts.GetValueOrDefault(ApplicationRoles.Employee),
            TotalAdmins = roleCounts.GetValueOrDefault(ApplicationRoles.Admin),
            TotalProducts = await _dbContext.Products.AsNoTracking().CountAsync(cancellationToken),
            ActiveProducts = await _dbContext.Products.AsNoTracking().CountAsync(product => product.Status == ProductStatus.Active, cancellationToken),
            TotalOrders = await _dbContext.Orders.AsNoTracking().CountAsync(cancellationToken),
            PendingOrders = await _dbContext.Orders.AsNoTracking().CountAsync(order => order.Status == OrderStatus.Pending, cancellationToken),
            HiddenReviews = await _dbContext.ProductReviews.AsNoTracking().CountAsync(review => review.Status == ProductReviewStatus.Hidden, cancellationToken),
            TotalRevenue = await _dbContext.Orders.AsNoTracking()
                .Where(order => order.Status != OrderStatus.Cancelled)
                .SumAsync(order => (decimal?)order.Subtotal, cancellationToken) ?? 0m,
            RecentOrders = recentOrders,
            LowStockProducts = lowStockProducts
        };
    }
}