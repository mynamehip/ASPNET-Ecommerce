using System.ComponentModel.DataAnnotations;
using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class ReviewStatusUpdateViewModel
{
    [Range(1, int.MaxValue)]
    public int ReviewId { get; set; }

    [EnumDataType(typeof(ProductReviewStatus))]
    [Display(Name = "Review status")]
    public ProductReviewStatus Status { get; set; }

    [StringLength(2000)]
    [Display(Name = "Admin reply")]
    public string? AdminReply { get; set; }
}