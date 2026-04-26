using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class ReviewAdminListViewModel
{
    public string? Search { get; set; }

    public ProductReviewStatus? Status { get; set; }

    public IReadOnlyList<ReviewAdminListItemViewModel> Reviews { get; set; } = [];
}