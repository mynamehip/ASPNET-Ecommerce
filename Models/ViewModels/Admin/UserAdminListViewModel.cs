using ASPNET_Ecommerce.Models.Identity;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class UserAdminListViewModel
{
    public string? Search { get; set; }

    public string? Role { get; set; }

    public IReadOnlyList<string> AvailableRoles { get; set; } = ApplicationRoles.All;

    public IReadOnlyList<UserAdminListItemViewModel> Users { get; set; } = [];
}