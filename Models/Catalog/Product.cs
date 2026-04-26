using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ASPNET_Ecommerce.Services;

namespace ASPNET_Ecommerce.Models.Catalog;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(64)]
    public string? Sku { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    [Range(0.01d, 999999999d, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    public bool IsDiscountActive { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? DiscountPercentage { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<ProductImage> Images { get; set; } = [];

    public ICollection<ProductReview> Reviews { get; set; } = [];

    [NotMapped]
    public bool HasActiveDiscount => ProductPricing.HasActiveDiscount(this);

    [NotMapped]
    public decimal EffectivePrice => ProductPricing.GetEffectivePrice(this);

    [NotMapped]
    public decimal DiscountAmount => ProductPricing.GetDiscountAmount(this);
}
