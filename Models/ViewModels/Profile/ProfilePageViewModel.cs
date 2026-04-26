namespace ASPNET_Ecommerce.Models.ViewModels.Profile;

public class ProfilePageViewModel
{
    public ProfileEditViewModel ProfileForm { get; set; } = new();

    public ChangePasswordViewModel ChangePasswordForm { get; set; } = new();

    public IReadOnlyList<string> Roles { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public bool EmailConfirmed { get; set; }
}