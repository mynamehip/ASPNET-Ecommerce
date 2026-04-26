using System.ComponentModel.DataAnnotations;
using ASPNET_Ecommerce.Models.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(64)]
    [Display(Name = "SKU")]
    public string? Sku { get; set; }

    [StringLength(4000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Range(0.01d, 999999999d, ErrorMessage = "Price must be greater than 0.")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [Display(Name = "Enable direct discount")]
    public bool IsDiscountActive { get; set; }

    [Range(typeof(decimal), "0", "100")]
    [Display(Name = "Discount percentage")]
    public decimal? DiscountPercentage { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Stock quantity")]
    public int StockQuantity { get; set; }

    [Display(Name = "Status")]
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    [Range(1, int.MaxValue, ErrorMessage = "Please choose a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Upload images")]
    public List<IFormFile> UploadedImages { get; set; } = [];

    public List<int> RemovedImageIds { get; set; } = [];

    [Display(Name = "Primary image")]
    public int? PrimaryImageId { get; set; }

    public List<ProductImageViewModel> ExistingImages { get; set; } = [];

    public IEnumerable<SelectListItem> Categories { get; set; } = [];
}
