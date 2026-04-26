using ASPNET_Ecommerce.Models.ViewModels.Admin;

namespace ASPNET_Ecommerce.Services;

public interface IUserAdminService
{
    Task<UserAdminListViewModel> GetListAsync(string? search, string? role, CancellationToken cancellationToken = default);

    Task<UserAdminDetailsViewModel?> GetDetailsAsync(string userId, string currentUserId, CancellationToken cancellationToken = default);

    Task UpdateAsync(string actorUserId, UserAdminUpdateViewModel model, CancellationToken cancellationToken = default);
}