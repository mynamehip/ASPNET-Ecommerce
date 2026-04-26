using ASPNET_Ecommerce.Models.ViewModels.Admin;

namespace ASPNET_Ecommerce.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetAsync(CancellationToken cancellationToken = default);
}