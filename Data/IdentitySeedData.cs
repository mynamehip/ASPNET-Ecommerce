using ASPNET_Ecommerce.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace ASPNET_Ecommerce.Data;

public static class IdentitySeedData
{
    public const string AdminEmail = "admin@aspnetecommerce.local";
    public const string AdminPassword = "Admin@12345";
    public const string AdminFullName = "System Administrator";
    public const string EmployeeEmail = "employee@aspnetecommerce.local";
    public const string EmployeePassword = "Employee@12345";
    public const string EmployeeFullName = "Catalog Employee";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = AdminFullName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Unable to seed admin account: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
        }

        var employeeUser = await userManager.FindByEmailAsync(EmployeeEmail);
        if (employeeUser is null)
        {
            employeeUser = new ApplicationUser
            {
                UserName = EmployeeEmail,
                Email = EmployeeEmail,
                FullName = EmployeeFullName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(employeeUser, EmployeePassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Unable to seed employee account: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(employeeUser, ApplicationRoles.Employee))
        {
            await userManager.AddToRoleAsync(employeeUser, ApplicationRoles.Employee);
        }
    }
}