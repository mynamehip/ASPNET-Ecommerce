using System.ComponentModel.DataAnnotations;

namespace ASPNET_Ecommerce.Models.ViewModels.Products;

public class ReviewFormViewModel
{
    public int ProductId { get; set; }

    [Range(1, 5, ErrorMessage = "Please select a rating between 1 and 5.")]
    [Display(Name = "Rating")]
    public int Rating { get; set; }

    [StringLength(2000)]
    [Display(Name = "Comment")]
    public string? Comment { get; set; }
}
