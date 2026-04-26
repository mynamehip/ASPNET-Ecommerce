namespace ASPNET_Ecommerce.Models.Identity;

public enum ApplicationRole
{
    Admin = 1,
    Employee = 2,
    User = 3
}

public static class ApplicationRoles
{
    public const string Admin = nameof(ApplicationRole.Admin);
    public const string Employee = nameof(ApplicationRole.Employee);
    public const string User = nameof(ApplicationRole.User);

    public static readonly string[] All =
    [
        Admin,
        Employee,
        User
    ];
}