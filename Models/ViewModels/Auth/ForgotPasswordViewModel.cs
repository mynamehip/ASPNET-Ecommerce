using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.ViewModels.Auth;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}