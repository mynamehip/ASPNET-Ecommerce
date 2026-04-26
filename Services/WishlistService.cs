using System.Security.Claims;
using System.Text.Json;
using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Wishlist;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class WishlistService : IWishlistService
{
    private const string WishlistSessionKey = "wishlist.items";

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WishlistService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<WishlistIndexViewModel> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await MergeSessionIntoDatabaseAsync(userId, cancellationToken);
            return await GetDatabaseWishlistAsync(userId, cancellationToken);
        }

        return await GetSessionWishlistAsync(cancellationToken);
    }

    public async Task<int> GetItemCountAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await MergeSessionIntoDatabaseAsync(userId, cancellationToken);
            return await _dbContext.Set<WishlistItem>()
                .AsNoTracking()
                .CountAsync(item => item.UserId == userId, cancellationToken);
        }

        return GetSessionItems().Count;
    }

    public async Task<bool> ContainsAsync(int productId, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await MergeSessionIntoDatabaseAsync(userId, cancellationToken);
            return await _dbContext.Set<WishlistItem>()
                .AsNoTracking()
                .AnyAsync(item => item.UserId == userId && item.ProductId == productId, cancellationToken);
        }

        return GetSessionItems().Any(item => item.ProductId == productId);
    }

    public async Task AddAsync(int productId, CancellationToken cancellationToken = default)
    {
        await EnsureActiveProductAsync(productId, cancellationToken);

        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await MergeSessionIntoDatabaseAsync(userId, cancellationToken);

            var exists = await _dbContext.Set<WishlistItem>()
                .AsNoTracking()
                .AnyAsync(item => item.UserId == userId && item.ProductId == productId, cancellationToken);

            if (exists)
            {
                return;
            }

            _dbContext.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var sessionItems = GetSessionItems();
        if (sessionItems.Any(item => item.ProductId == productId))
        {
            return;
        }

        sessionItems.Insert(0, new WishlistSessionItem
        {
            ProductId = productId,
            CreatedAtUtc = DateTime.UtcNow
        });

        SaveSessionItems(sessionItems);
    }

    public async Task RemoveAsync(int productId, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await MergeSessionIntoDatabaseAsync(userId, cancellationToken);

            var item = await _dbContext.Set<WishlistItem>()
                .SingleOrDefaultAsync(entry => entry.UserId == userId && entry.ProductId == productId, cancellationToken);

            if (item is null)
            {
                return;
            }

            _dbContext.Remove(item);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var sessionItems = GetSessionItems();
        var removed = sessionItems.RemoveAll(item => item.ProductId == productId);
        if (removed > 0)
        {
            SaveSessionItems(sessionItems);
        }
    }

    private async Task<WishlistIndexViewModel> GetDatabaseWishlistAsync(string userId, CancellationToken cancellationToken)
    {
        var items = await _dbContext.Set<WishlistItem>()
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .Include(item => item.Product!)
                .ThenInclude(product => product!.Category)
            .Include(item => item.Product!)
                .ThenInclude(product => product!.Images)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Select(item => new WishlistItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.Product!.Name,
                ProductSku = item.Product.Sku,
                CategoryName = item.Product.Category != null ? item.Product.Category.Name : null,
                ImagePath = item.Product.Images
                    .OrderBy(image => image.DisplayOrder)
                    .Select(image => image.ImagePath)
                    .FirstOrDefault(),
                OriginalPrice = item.Product.Price,
                EffectivePrice = ProductPricing.GetEffectivePrice(item.Product),
                IsDiscountActive = ProductPricing.HasActiveDiscount(item.Product),
                DiscountPercentage = item.Product.DiscountPercentage,
                StockQuantity = item.Product.StockQuantity,
                IsActive = item.Product.Status == ProductStatus.Active,
                AddedAtUtc = item.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new WishlistIndexViewModel
        {
            Items = items
        };
    }

    private async Task<WishlistIndexViewModel> GetSessionWishlistAsync(CancellationToken cancellationToken)
    {
        var sessionItems = GetSessionItems();
        if (sessionItems.Count == 0)
        {
            return new WishlistIndexViewModel();
        }

        var productIds = sessionItems
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => productIds.Contains(product.Id))
            .Include(product => product.Category)
            .Include(product => product.Images)
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        var normalizedSessionItems = new List<WishlistSessionItem>(sessionItems.Count);
        var items = new List<WishlistItemViewModel>(sessionItems.Count);

        foreach (var sessionItem in sessionItems)
        {
            if (!products.TryGetValue(sessionItem.ProductId, out var product))
            {
                continue;
            }

            normalizedSessionItems.Add(sessionItem);
            items.Add(new WishlistItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                CategoryName = product.Category?.Name,
                ImagePath = product.Images
                    .OrderBy(image => image.DisplayOrder)
                    .Select(image => image.ImagePath)
                    .FirstOrDefault(),
                OriginalPrice = product.Price,
                EffectivePrice = ProductPricing.GetEffectivePrice(product),
                IsDiscountActive = ProductPricing.HasActiveDiscount(product),
                DiscountPercentage = product.DiscountPercentage,
                StockQuantity = product.StockQuantity,
                IsActive = product.Status == ProductStatus.Active,
                AddedAtUtc = sessionItem.CreatedAtUtc
            });
        }

        if (normalizedSessionItems.Count != sessionItems.Count)
        {
            SaveSessionItems(normalizedSessionItems);
        }

        return new WishlistIndexViewModel
        {
            Items = items
        };
    }

    private async Task MergeSessionIntoDatabaseAsync(string userId, CancellationToken cancellationToken)
    {
        var sessionItems = GetSessionItems();
        if (sessionItems.Count == 0)
        {
            return;
        }

        var sessionProductIds = sessionItems
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();

        var existingProductIds = await _dbContext.Set<WishlistItem>()
            .AsNoTracking()
            .Where(item => item.UserId == userId && sessionProductIds.Contains(item.ProductId))
            .Select(item => item.ProductId)
            .ToListAsync(cancellationToken);

        var activeProductIds = await _dbContext.Products
            .AsNoTracking()
            .Where(product => sessionProductIds.Contains(product.Id) && product.Status == ProductStatus.Active)
            .Select(product => product.Id)
            .ToListAsync(cancellationToken);

        var activeProductIdSet = activeProductIds.ToHashSet();
        var existingProductIdSet = existingProductIds.ToHashSet();
        var pendingItems = sessionItems
            .Where(item => activeProductIdSet.Contains(item.ProductId) && !existingProductIdSet.Contains(item.ProductId))
            .ToList();

        if (pendingItems.Count > 0)
        {
            foreach (var sessionItem in pendingItems)
            {
                _dbContext.Add(new WishlistItem
                {
                    UserId = userId,
                    ProductId = sessionItem.ProductId,
                    CreatedAtUtc = sessionItem.CreatedAtUtc
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        SaveSessionItems([]);
    }

    private async Task EnsureActiveProductAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);

        if (product is null || product.Status != ProductStatus.Active)
        {
            throw new InvalidOperationException("This product is no longer available for wishlist.");
        }
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private List<WishlistSessionItem> GetSessionItems()
    {
        var json = GetSession().GetString(WishlistSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<WishlistSessionItem>>(json) ?? [];
    }

    private void SaveSessionItems(List<WishlistSessionItem> items)
    {
        var session = GetSession();
        if (items.Count == 0)
        {
            session.Remove(WishlistSessionKey);
            return;
        }

        session.SetString(WishlistSessionKey, JsonSerializer.Serialize(items));
    }

    private ISession GetSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            throw new InvalidOperationException("Session is not available for the current request.");
        }

        return session;
    }

    private sealed class WishlistSessionItem
    {
        public int ProductId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
