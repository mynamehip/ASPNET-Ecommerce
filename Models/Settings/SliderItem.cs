using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.Settings;

public class SliderItem
{
    public int Id { get; set; }

    public SliderItemType ItemType { get; set; } = SliderItemType.Slide;

    [StringLength(120)]
    public string? Content { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(260)]
    public string? PrimaryButtonUrl { get; set; }

    [StringLength(260)]
    public string? SecondaryButtonUrl { get; set; }

    [StringLength(260)]
    public string? BackgroundImagePath { get; set; }

    [StringLength(260)]
    public string? ClickUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
