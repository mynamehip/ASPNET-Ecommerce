using ASPNET_Ecommerce.Models.Settings;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class SliderListItemViewModel
{
    public int Id { get; set; }

    public SliderItemType ItemType { get; set; }

    public string? Content { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? PrimaryButtonUrl { get; set; }

    public string? SecondaryButtonUrl { get; set; }

    public string? BackgroundImagePath { get; set; }

    public string? ClickUrl { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }
}
