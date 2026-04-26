namespace ASPNET_Ecommerce.Models.Api;

public class ProductDetailDto : ProductSummaryDto
{
    public string? Description { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public IReadOnlyList<string> ImagePaths { get; set; } = [];

    public IReadOnlyList<ProductSummaryDto> RelatedProducts { get; set; } = [];
}