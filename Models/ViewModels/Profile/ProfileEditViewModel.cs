using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.ViewModels.Profile;

public class ProfileEditViewModel
{
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

    [StringLength(200)]
    [Display(Name = "Default address")]
    public string? DefaultAddress { get; set; }

    [StringLength(120)]
    [Display(Name = "Default ward")]
    public string? DefaultWard { get; set; }

    [StringLength(120)]
    [Display(Name = "Default province")]
    public string? DefaultProvince { get; set; }
}