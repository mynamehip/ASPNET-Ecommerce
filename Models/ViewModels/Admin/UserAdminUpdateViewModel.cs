using System.ComponentModel.DataAnnotations;
using ASPNET_Ecommerce.Models.Identity;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class UserAdminUpdateViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required]
    [Display(Name = "Role")]
    public string SelectedRole { get; set; } = ApplicationRoles.User;

    [Display(Name = "Email confirmed")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Lock this account")]
    public bool IsLockedOut { get; set; }
}