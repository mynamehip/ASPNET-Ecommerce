using System.ComponentModel.DataAnnotations;
using ASPNET_Ecommerce.Models.Settings;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class SliderFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Slide type")]
    public SliderItemType ItemType { get; set; } = SliderItemType.Slide;

    [StringLength(120)]
    [Display(Name = "Content")]
    public string? Content { get; set; }

    [StringLength(200)]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    [StringLength(1000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(260)]
    [Display(Name = "Primary button URL")]
    public string? PrimaryButtonUrl { get; set; }

    [StringLength(260)]
    [Display(Name = "Secondary button URL")]
    public string? SecondaryButtonUrl { get; set; }

    public string? ExistingBackgroundImagePath { get; set; }

    [Display(Name = "Background image")]
    public IFormFile? BackgroundImageFile { get; set; }

    [Display(Name = "Remove current background")]
    public bool RemoveBackgroundImage { get; set; }

    [StringLength(260)]
    [Display(Name = "Click URL")]
    public string? ClickUrl { get; set; }

    [Display(Name = "Visible")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    public bool IsBanner => ItemType == SliderItemType.Banner;
}
