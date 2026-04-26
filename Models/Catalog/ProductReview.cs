using System.ComponentModel.DataAnnotations;
using ASPNET_Ecommerce.Models.Identity;

namespace ASPNET_Ecommerce.Models.Catalog;

public class ProductReview
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    [StringLength(2000)]
    public string? AdminReply { get; set; }

    public ProductReviewStatus Status { get; set; } = ProductReviewStatus.Approved;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? AdminRepliedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
