using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Range(0, 9999)]
    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}