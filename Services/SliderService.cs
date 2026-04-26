using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Settings;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class SliderService : ISliderService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".svg"
    };

    private const long MaxImageSizeBytes = 5 * 1024 * 1024;
    private const string UploadFolder = "uploads/slider";

    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public SliderService(ApplicationDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<SliderIndexViewModel> GetAdminIndexAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.SliderItems
            .AsNoTracking()
            .OrderBy(item => item.ItemType)
            .ThenBy(item => item.DisplayOrder)
            .ThenBy(item => item.Id)
            .Select(item => new SliderListItemViewModel
            {
                Id = item.Id,
                ItemType = item.ItemType,
                Content = item.Content,
                Title = item.Title,
                Description = item.Description,
                PrimaryButtonUrl = item.PrimaryButtonUrl,
                SecondaryButtonUrl = item.SecondaryButtonUrl,
                BackgroundImagePath = item.BackgroundImagePath,
                ClickUrl = item.ClickUrl,
                IsActive = item.IsActive,
                DisplayOrder = item.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new SliderIndexViewModel
        {
            BannerItems = items.Where(item => item.ItemType == SliderItemType.Banner).ToList(),
            SlideItems = items.Where(item => item.ItemType == SliderItemType.Slide).ToList()
        };
    }

    public Task<SliderFormViewModel> BuildCreateModelAsync(SliderItemType itemType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SliderFormViewModel
        {
            ItemType = itemType,
            IsActive = true,
            DisplayOrder = 0
        });
    }

    public async Task<SliderFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.SliderItems
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (item is null)
        {
            return null;
        }

        return new SliderFormViewModel
        {
            Id = item.Id,
            ItemType = item.ItemType,
            Content = item.Content,
            Title = item.Title,
            Description = item.Description,
            PrimaryButtonUrl = item.PrimaryButtonUrl,
            SecondaryButtonUrl = item.SecondaryButtonUrl,
            ExistingBackgroundImagePath = item.BackgroundImagePath,
            ClickUrl = item.ClickUrl,
            IsActive = item.IsActive,
            DisplayOrder = item.DisplayOrder
        };
    }

    public async Task CreateAsync(SliderFormViewModel model, CancellationToken cancellationToken = default)
    {
        var normalized = await NormalizeAsync(model, existingBackgroundImagePath: null, cancellationToken);

        var entity = new SliderItem
        {
            ItemType = model.ItemType,
            Content = normalized.Content,
            Title = normalized.Title,
            Description = normalized.Description,
            PrimaryButtonUrl = normalized.PrimaryButtonUrl,
            SecondaryButtonUrl = normalized.SecondaryButtonUrl,
            BackgroundImagePath = normalized.BackgroundImagePath,
            ClickUrl = normalized.ClickUrl,
            IsActive = model.IsActive,
            DisplayOrder = model.DisplayOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.SliderItems.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SliderFormViewModel model, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.SliderItems.SingleOrDefaultAsync(item => item.Id == model.Id, cancellationToken);
        if (entity is null)
        {
            throw new InvalidOperationException("Slider item not found.");
        }

        if (entity.ItemType != model.ItemType)
        {
            throw new InvalidOperationException("Changing slider item type is not supported.");
        }

        var previousBackgroundImagePath = entity.BackgroundImagePath;
        var normalized = await NormalizeAsync(model, entity.BackgroundImagePath, cancellationToken);

        entity.Content = normalized.Content;
        entity.Title = normalized.Title;
        entity.Description = normalized.Description;
        entity.PrimaryButtonUrl = normalized.PrimaryButtonUrl;
        entity.SecondaryButtonUrl = normalized.SecondaryButtonUrl;
        entity.BackgroundImagePath = normalized.BackgroundImagePath;
        entity.ClickUrl = normalized.ClickUrl;
        entity.IsActive = model.IsActive;
        entity.DisplayOrder = model.DisplayOrder;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousBackgroundImagePath) && previousBackgroundImagePath != entity.BackgroundImagePath)
        {
            DeletePhysicalFile(previousBackgroundImagePath);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.SliderItems.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            throw new InvalidOperationException("Slider item not found.");
        }

        _dbContext.SliderItems.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(entity.BackgroundImagePath))
        {
            DeletePhysicalFile(entity.BackgroundImagePath);
        }
    }

    private async Task<NormalizedSliderModel> NormalizeAsync(
        SliderFormViewModel model,
        string? existingBackgroundImagePath,
        CancellationToken cancellationToken)
    {
        var content = NormalizeText(model.Content);
        var title = NormalizeText(model.Title);
        var description = NormalizeText(model.Description);
        var primaryButtonUrl = NormalizeUrl(model.PrimaryButtonUrl, "Primary button URL");
        var secondaryButtonUrl = NormalizeUrl(model.SecondaryButtonUrl, "Secondary button URL");
        var clickUrl = NormalizeUrl(model.ClickUrl, "Click URL");
        var backgroundImagePath = model.RemoveBackgroundImage ? null : existingBackgroundImagePath;

        if (model.BackgroundImageFile is not null && model.BackgroundImageFile.Length > 0)
        {
            backgroundImagePath = await SaveBackgroundImageAsync(model.BackgroundImageFile, cancellationToken);
        }

        ValidateByType(model.ItemType, content, title, description, primaryButtonUrl, clickUrl, backgroundImagePath);

        return new NormalizedSliderModel
        {
            Content = content,
            Title = title,
            Description = description,
            PrimaryButtonUrl = primaryButtonUrl,
            SecondaryButtonUrl = secondaryButtonUrl,
            ClickUrl = clickUrl,
            BackgroundImagePath = backgroundImagePath
        };
    }

    private static void ValidateByType(
        SliderItemType itemType,
        string? content,
        string? title,
        string? description,
        string? primaryButtonUrl,
        string? clickUrl,
        string? backgroundImagePath)
    {
        if (itemType == SliderItemType.Banner)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Banner content is required.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new InvalidOperationException("Banner title is required.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new InvalidOperationException("Banner description is required.");
            }

            if (string.IsNullOrWhiteSpace(primaryButtonUrl))
            {
                throw new InvalidOperationException("Banner primary button URL is required.");
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(backgroundImagePath))
        {
            throw new InvalidOperationException("Slide background image is required.");
        }
    }

    private async Task<string> SaveBackgroundImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ValidateBackgroundImage(file);

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            throw new InvalidOperationException("Web root path is not configured for slider upload.");
        }

        var uploadRoot = Path.Combine(webRootPath, "uploads", "slider");
        Directory.CreateDirectory(uploadRoot);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"slider-{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadRoot, fileName);

        await using var stream = File.Create(physicalPath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/{UploadFolder}/{fileName}".Replace("\\", "/");
    }

    private static void ValidateBackgroundImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only .jpg, .jpeg, .png, .webp, and .svg files are allowed for slider backgrounds.");
        }

        if (file.Length > MaxImageSizeBytes)
        {
            throw new InvalidOperationException("Slider background image must be 5 MB or smaller.");
        }
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeUrl(string? value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.StartsWith('/'))
        {
            return normalized;
        }

        if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri)
            && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            return normalized;
        }

        throw new InvalidOperationException($"{label} must be a relative path starting with '/' or an absolute http/https URL.");
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

    private sealed class NormalizedSliderModel
    {
        public string? Content { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? PrimaryButtonUrl { get; set; }

        public string? SecondaryButtonUrl { get; set; }

        public string? BackgroundImagePath { get; set; }

        public string? ClickUrl { get; set; }
    }
}
