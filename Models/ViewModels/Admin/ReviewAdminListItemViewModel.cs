using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class ReviewAdminListItemViewModel
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string UserFullName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string? CommentPreview { get; set; }

    public ProductReviewStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}