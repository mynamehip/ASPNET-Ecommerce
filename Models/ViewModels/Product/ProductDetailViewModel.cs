using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Models.ViewModels.Products;

public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public IReadOnlyList<Product> RelatedProducts { get; set; } = [];
    public IReadOnlyList<ProductReview> Reviews { get; set; } = [];
    public bool IsInWishlist { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool UserHasReviewed { get; set; }
    public bool CanCurrentUserReview { get; set; }
    public string? ReviewEligibilityMessage { get; set; }
    public ProductReviewStatus? CurrentUserReviewStatus { get; set; }
    public ReviewFormViewModel ReviewForm { get; set; } = new();
}
