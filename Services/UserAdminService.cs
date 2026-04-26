using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Identity;
using ASPNET_Ecommerce.Models.Orders;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class UserAdminService : IUserAdminService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAdminService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<UserAdminListViewModel> GetListAsync(string? search, string? role, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user =>
                user.FullName.Contains(term) ||
                (user.Email != null && user.Email.Contains(term)) ||
                (user.PhoneNumber != null && user.PhoneNumber.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(role) && ApplicationRoles.All.Contains(role))
        {
            var selectedRole = role.Trim();
            query = query.Where(user =>
                (from userRole in _dbContext.UserRoles
                 join appRole in _dbContext.Roles on userRole.RoleId equals appRole.Id
                 where userRole.UserId == user.Id && appRole.Name == selectedRole
                 select appRole.Id)
                .Any());
        }

        var users = await query
            .OrderByDescending(user => user.CreatedAtUtc)
            .ThenBy(user => user.FullName)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(user => user.Id).ToList();
        var roleLookup = await BuildRoleLookupAsync(userIds, cancellationToken);
        var orderCounts = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId != null && userIds.Contains(order.UserId))
            .GroupBy(order => order.UserId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Key!, item => item.Count, cancellationToken);

        return new UserAdminListViewModel
        {
            Search = search,
            Role = role,
            Users = users.Select(user => new UserAdminListItemViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = roleLookup.GetValueOrDefault(user.Id, ApplicationRoles.User),
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                TotalOrders = orderCounts.GetValueOrDefault(user.Id),
                CreatedAtUtc = user.CreatedAtUtc
            }).ToList()
        };
    }

    public async Task<UserAdminDetailsViewModel?> GetDetailsAsync(string userId, string currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleLookup = await BuildRoleLookupAsync([userId], cancellationToken);
        var orderStats = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .GroupBy(order => order.UserId)
            .Select(group => new
            {
                TotalOrders = group.Count(),
                CompletedOrders = group.Count(order => order.Status == OrderStatus.Completed),
                TotalSpent = group.Where(order => order.Status != OrderStatus.Cancelled).Sum(order => order.Subtotal),
                LastOrderAtUtc = group.Max(order => (DateTime?)order.CreatedAtUtc)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var role = roleLookup.GetValueOrDefault(userId, ApplicationRoles.User);

        return new UserAdminDetailsViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            CurrentRole = role,
            EmailConfirmed = user.EmailConfirmed,
            IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
            IsCurrentUser = string.Equals(user.Id, currentUserId, StringComparison.Ordinal),
            CreatedAtUtc = user.CreatedAtUtc,
            TotalOrders = orderStats?.TotalOrders ?? 0,
            CompletedOrders = orderStats?.CompletedOrders ?? 0,
            TotalSpent = orderStats?.TotalSpent ?? 0m,
            LastOrderAtUtc = orderStats?.LastOrderAtUtc,
            UpdateForm = new UserAdminUpdateViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                SelectedRole = role,
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
            }
        };
    }

    public async Task UpdateAsync(string actorUserId, UserAdminUpdateViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ApplicationRoles.All.Contains(model.SelectedRole))
        {
            throw new InvalidOperationException("Invalid user role.");
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null)
        {
            throw new InvalidOperationException("User account not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault() ?? ApplicationRoles.User;

        if (string.Equals(actorUserId, user.Id, StringComparison.Ordinal) && !string.Equals(model.SelectedRole, ApplicationRoles.Admin, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("You cannot remove your own administrator access.");
        }

        if (string.Equals(actorUserId, user.Id, StringComparison.Ordinal) && model.IsLockedOut)
        {
            throw new InvalidOperationException("You cannot lock your own account.");
        }

        if (string.Equals(currentRole, ApplicationRoles.Admin, StringComparison.Ordinal) && !string.Equals(model.SelectedRole, ApplicationRoles.Admin, StringComparison.Ordinal))
        {
            var adminCount = await CountUsersInRoleAsync(ApplicationRoles.Admin, cancellationToken);
            if (adminCount <= 1)
            {
                throw new InvalidOperationException("At least one administrator account must remain assigned.");
            }
        }

        if (string.Equals(currentRole, ApplicationRoles.Admin, StringComparison.Ordinal) && model.IsLockedOut)
        {
            var activeAdminCountExcludingCurrent = await CountUnlockedAdminsExcludingAsync(user.Id, cancellationToken);
            if (activeAdminCountExcludingCurrent == 0)
            {
                throw new InvalidOperationException("At least one unlocked administrator account must remain available.");
            }
        }

        var normalizedEmail = model.Email.Trim();
        var phoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();

        var duplicateEmailExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(item => item.Id != user.Id && item.NormalizedEmail == normalizedEmail.ToUpperInvariant(), cancellationToken);

        if (duplicateEmailExists)
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        user.FullName = model.FullName.Trim();
        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;
        user.PhoneNumber = phoneNumber;
        user.EmailConfirmed = model.EmailConfirmed;
        user.LockoutEnabled = true;
        user.LockoutEnd = model.IsLockedOut ? DateTimeOffset.MaxValue : null;

        var updateResult = await _userManager.UpdateAsync(user);
        EnsureSuccess(updateResult, "Unable to update the user account.");

        if (currentRoles.Count != 1 || !string.Equals(currentRole, model.SelectedRole, StringComparison.Ordinal))
        {
            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                EnsureSuccess(removeResult, "Unable to update the user role.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            EnsureSuccess(addResult, "Unable to update the user role.");
        }
    }

    private async Task<Dictionary<string, string>> BuildRoleLookupAsync(IReadOnlyCollection<string> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var roles = await (from userRole in _dbContext.UserRoles
                           join appRole in _dbContext.Roles on userRole.RoleId equals appRole.Id
                           where userIds.Contains(userRole.UserId)
                           select new
                           {
                               userRole.UserId,
                               RoleName = appRole.Name ?? ApplicationRoles.User
                           })
            .ToListAsync(cancellationToken);

        return roles
            .GroupBy(item => item.UserId)
            .ToDictionary(group => group.Key, group => group.Select(item => item.RoleName).FirstOrDefault() ?? ApplicationRoles.User);
    }

    private async Task<int> CountUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        return await (from userRole in _dbContext.UserRoles
                      join role in _dbContext.Roles on userRole.RoleId equals role.Id
                      where role.Name == roleName
                      select userRole.UserId)
            .CountAsync(cancellationToken);
    }

    private async Task<int> CountUnlockedAdminsExcludingAsync(string excludedUserId, CancellationToken cancellationToken)
    {
        return await (from user in _dbContext.Users
                      join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                      join role in _dbContext.Roles on userRole.RoleId equals role.Id
                      where role.Name == ApplicationRoles.Admin
                            && user.Id != excludedUserId
                            && (!user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow)
                      select user.Id)
            .CountAsync(cancellationToken);
    }

    private static void EnsureSuccess(IdentityResult result, string fallbackMessage)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = result.Errors.Select(error => error.Description).ToArray();
        throw new InvalidOperationException(errors.Length > 0 ? string.Join(" ", errors) : fallbackMessage);
    }
}