using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class ReviewAdminDetailsViewModel
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string UserFullName { get; set; } = string.Empty;

    public string? UserEmail { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string? AdminReply { get; set; }

    public ProductReviewStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? AdminRepliedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ReviewStatusUpdateViewModel StatusForm { get; set; } = new();
}