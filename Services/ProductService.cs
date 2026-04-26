using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class ProductService : IProductService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxImageSizeBytes = 5 * 1024 * 1024;
    private const string UploadFolder = "uploads/products";

    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ProductService(ApplicationDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<IReadOnlyList<AdminProductListItemViewModel>> GetAdminListAsync(CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Images)
            .OrderByDescending(product => product.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return products
            .Select(product =>
            {
                var primaryImage = product.Images.OrderBy(image => image.DisplayOrder).FirstOrDefault(image => image.IsPrimary)
                    ?? product.Images.OrderBy(image => image.DisplayOrder).FirstOrDefault();

                return new AdminProductListItemViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Sku = product.Sku,
                    CategoryName = product.Category?.Name,
                    OriginalPrice = product.Price,
                    EffectivePrice = ProductPricing.GetEffectivePrice(product.Price, product.IsDiscountActive, product.DiscountPercentage),
                    IsDiscountActive = product.IsDiscountActive && product.DiscountPercentage.HasValue && product.DiscountPercentage.Value > 0,
                    DiscountPercentage = product.DiscountPercentage,
                    StockQuantity = product.StockQuantity,
                    Status = product.Status,
                    ImageCount = product.Images.Count,
                    PrimaryImagePath = primaryImage?.ImagePath
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Images)
            .OrderByDescending(product => product.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 8, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Status == ProductStatus.Active)
            .Include(product => product.Category)
            .Include(product => product.Images)
            .OrderByDescending(product => product.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetNewArrivalsAsync(int count = 8, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Status == ProductStatus.Active)
            .Include(product => product.Category)
            .Include(product => product.Images)
            .OrderByDescending(product => product.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetDiscountedAsync(int count = 8, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Status == ProductStatus.Active
                && product.IsDiscountActive
                && product.DiscountPercentage.HasValue
                && product.DiscountPercentage.Value > 0)
            .Include(product => product.Category)
            .Include(product => product.Images)
            .OrderByDescending(product => product.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetActiveAsync(
        string? search = null,
        int? categoryId = null,
        string? sortBy = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool inStockOnly = false,
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Status == ProductStatus.Active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term)
                                  || (p.Sku != null && p.Sku.ToLower().Contains(term))
                                  || (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p =>
                (p.IsDiscountActive && p.DiscountPercentage.HasValue && p.DiscountPercentage.Value > 0
                    ? p.Price * (1 - (p.DiscountPercentage.Value / 100m))
                    : p.Price) >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p =>
                (p.IsDiscountActive && p.DiscountPercentage.HasValue && p.DiscountPercentage.Value > 0
                    ? p.Price * (1 - (p.DiscountPercentage.Value / 100m))
                    : p.Price) <= maxPrice.Value);
        }

        if (inStockOnly)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p =>
                p.IsDiscountActive && p.DiscountPercentage.HasValue && p.DiscountPercentage.Value > 0
                    ? p.Price * (1 - (p.DiscountPercentage.Value / 100m))
                    : p.Price),
            "price_desc" => query.OrderByDescending(p =>
                p.IsDiscountActive && p.DiscountPercentage.HasValue && p.DiscountPercentage.Value > 0
                    ? p.Price * (1 - (p.DiscountPercentage.Value / 100m))
                    : p.Price),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAtUtc)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .SingleOrDefaultAsync(p => p.Id == id && p.Status == ProductStatus.Active, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(int productId, int categoryId, int count = 4, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active && p.Id != productId && p.CategoryId == categoryId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductFormViewModel> BuildCreateModelAsync(CancellationToken cancellationToken = default)
    {
        var model = new ProductFormViewModel();
        await PopulateLookupsAsync(model, cancellationToken);
        return model;
    }

    public async Task<ProductFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(item => item.Images)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            Price = product.Price,
            IsDiscountActive = product.IsDiscountActive,
            DiscountPercentage = product.DiscountPercentage,
            StockQuantity = product.StockQuantity,
            Status = product.Status,
            CategoryId = product.CategoryId,
            PrimaryImageId = product.Images.FirstOrDefault(image => image.IsPrimary)?.Id,
            ExistingImages = product.Images
                .OrderBy(image => image.DisplayOrder)
                .Select(image => new ProductImageViewModel
                {
                    Id = image.Id,
                    ImagePath = image.ImagePath,
                    IsPrimary = image.IsPrimary,
                    DisplayOrder = image.DisplayOrder
                })
                .ToList()
        };

        await PopulateLookupsAsync(model, cancellationToken);
        return model;
    }

    public async Task PopulateLookupsAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
    {
        model.Categories = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = $"{category.Name}{(category.IsActive ? string.Empty : " (Inactive)")}",
                Selected = category.Id == model.CategoryId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(model.CategoryId, cancellationToken);
        ValidateDiscount(model.Price, model.IsDiscountActive, model.DiscountPercentage);

        var imagePaths = await SaveUploadedFilesAsync(model.UploadedImages, cancellationToken);

        var product = new Product
        {
            Name = model.Name.Trim(),
            Sku = string.IsNullOrWhiteSpace(model.Sku) ? null : model.Sku.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            Price = model.Price,
            IsDiscountActive = model.IsDiscountActive,
            DiscountPercentage = NormalizeDiscountPercentage(model.IsDiscountActive, model.DiscountPercentage),
            StockQuantity = model.StockQuantity,
            Status = model.Status,
            CategoryId = model.CategoryId,
            CreatedAtUtc = DateTime.UtcNow,
            Images = imagePaths
                .Select((imagePath, index) => new ProductImage
                {
                    ImagePath = imagePath,
                    DisplayOrder = index,
                    IsPrimary = index == 0,
                    CreatedAtUtc = DateTime.UtcNow
                })
                .ToList()
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(item => item.Images)
            .SingleOrDefaultAsync(item => item.Id == model.Id, cancellationToken);

        if (product is null)
        {
            throw new InvalidOperationException("Product not found.");
        }

        await EnsureCategoryExistsAsync(model.CategoryId, cancellationToken);
        ValidateDiscount(model.Price, model.IsDiscountActive, model.DiscountPercentage);

        product.Name = model.Name.Trim();
        product.Sku = string.IsNullOrWhiteSpace(model.Sku) ? null : model.Sku.Trim();
        product.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        product.Price = model.Price;
        product.IsDiscountActive = model.IsDiscountActive;
        product.DiscountPercentage = NormalizeDiscountPercentage(model.IsDiscountActive, model.DiscountPercentage);
        product.StockQuantity = model.StockQuantity;
        product.Status = model.Status;
        product.CategoryId = model.CategoryId;
        product.UpdatedAtUtc = DateTime.UtcNow;

        var imagesToDelete = product.Images
            .Where(image => model.RemovedImageIds.Contains(image.Id))
            .ToList();

        foreach (var image in imagesToDelete)
        {
            product.Images.Remove(image);
            _dbContext.ProductImages.Remove(image);
        }

        var newImagePaths = await SaveUploadedFilesAsync(model.UploadedImages, cancellationToken);
        var nextDisplayOrder = product.Images.Count == 0 ? 0 : product.Images.Max(image => image.DisplayOrder) + 1;

        foreach (var imagePath in newImagePaths)
        {
            product.Images.Add(new ProductImage
            {
                ImagePath = imagePath,
                DisplayOrder = nextDisplayOrder++,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        foreach (var image in product.Images)
        {
            image.IsPrimary = model.PrimaryImageId.HasValue && image.Id == model.PrimaryImageId.Value;
        }

        if (!product.Images.Any(image => image.IsPrimary) && product.Images.Count > 0)
        {
            product.Images.OrderBy(image => image.DisplayOrder).First().IsPrimary = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var image in imagesToDelete)
        {
            DeletePhysicalFile(image.ImagePath);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(item => item.Images)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (product is null)
        {
            throw new InvalidOperationException("Product not found.");
        }

        var imagePaths = product.Images.Select(image => image.ImagePath).ToList();

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var imagePath in imagePaths)
        {
            DeletePhysicalFile(imagePath);
        }
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Selected category does not exist.");
        }
    }

    private async Task<List<string>> SaveUploadedFilesAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken)
    {
        var uploadedFiles = files
            .Where(file => file.Length > 0)
            .ToList();

        if (uploadedFiles.Count == 0)
        {
            return [];
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            throw new InvalidOperationException("Web root path is not configured for image upload.");
        }

        var uploadRoot = Path.Combine(webRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadRoot);

        var savedPaths = new List<string>(uploadedFiles.Count);

        foreach (var file in uploadedFiles)
        {
            ValidateImage(file);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadRoot, fileName);

            await using var stream = File.Create(physicalPath);
            await file.CopyToAsync(stream, cancellationToken);

            savedPaths.Add($"/{UploadFolder}/{fileName}".Replace("\\", "/"));
        }

        return savedPaths;
    }

    private static void ValidateImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only .jpg, .jpeg, .png, and .webp files are allowed.");
        }

        if (file.Length > MaxImageSizeBytes)
        {
            throw new InvalidOperationException("Each image must be 5 MB or smaller.");
        }
    }

    private static void ValidateDiscount(decimal basePrice, bool isDiscountActive, decimal? discountPercentage)
    {
        if (!isDiscountActive)
        {
            return;
        }

        if (!discountPercentage.HasValue || discountPercentage.Value <= 0)
        {
            throw new InvalidOperationException("Discount percentage must be greater than 0 when direct discount is enabled.");
        }

        var discountedPrice = basePrice * (1 - (discountPercentage.Value / 100m));
        if (discountedPrice < 0)
        {
            throw new InvalidOperationException("Discounted price cannot be negative.");
        }
    }

    private static decimal? NormalizeDiscountPercentage(bool isDiscountActive, decimal? discountPercentage)
    {
        return isDiscountActive && discountPercentage.HasValue && discountPercentage.Value > 0
            ? decimal.Round(discountPercentage.Value, 2, MidpointRounding.AwayFromZero)
            : null;
    }

    private void DeletePhysicalFile(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            return;
        }

        var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }
}
