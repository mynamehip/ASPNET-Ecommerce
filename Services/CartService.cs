using System.Text.Json;
using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Cart;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class CartService : ICartService
{
    private const string CartSessionKey = "cart.items";

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CartIndexViewModel> GetCartAsync(CancellationToken cancellationToken = default)
    {
        var sessionItems = GetSessionItems();
        if (sessionItems.Count == 0)
        {
            return new CartIndexViewModel();
        }

        var requestedQuantities = sessionItems.ToDictionary(item => item.ProductId, item => item.Quantity);
        var productIds = requestedQuantities.Keys.ToList();

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Status == ProductStatus.Active && productIds.Contains(product.Id))
            .Include(product => product.Images)
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        var normalizedSessionItems = new List<CartSessionItem>(products.Count);
        var cartItems = new List<CartItemViewModel>(products.Count);

        foreach (var product in products)
        {
            if (!requestedQuantities.TryGetValue(product.Id, out var requestedQuantity) || product.StockQuantity <= 0)
            {
                continue;
            }

            var quantity = Math.Min(requestedQuantity, product.StockQuantity);
            if (quantity <= 0)
            {
                continue;
            }

            normalizedSessionItems.Add(new CartSessionItem
            {
                ProductId = product.Id,
                Quantity = quantity
            });

            var primaryImage = product.Images
                .OrderBy(image => image.DisplayOrder)
                .FirstOrDefault(image => image.IsPrimary)
                ?? product.Images.OrderBy(image => image.DisplayOrder).FirstOrDefault();

            cartItems.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                ImagePath = primaryImage?.ImagePath,
                OriginalUnitPrice = product.Price,
                UnitPrice = ProductPricing.GetEffectivePrice(product),
                IsDiscountActive = ProductPricing.HasActiveDiscount(product),
                DiscountPercentage = product.DiscountPercentage,
                Quantity = quantity,
                AvailableStock = product.StockQuantity
            });
        }

        var sessionChanged = normalizedSessionItems.Count != sessionItems.Count;
        if (!sessionChanged)
        {
            for (var index = 0; index < normalizedSessionItems.Count; index++)
            {
                if (normalizedSessionItems[index].ProductId != sessionItems[index].ProductId
                    || normalizedSessionItems[index].Quantity != sessionItems[index].Quantity)
                {
                    sessionChanged = true;
                    break;
                }
            }
        }

        if (sessionChanged)
        {
            SaveSessionItems(normalizedSessionItems);
        }

        return new CartIndexViewModel
        {
            Items = cartItems,
            ItemCount = cartItems.Sum(item => item.Quantity),
            Subtotal = cartItems.Sum(item => item.LineTotal)
        };
    }

    public Task<int> GetItemCountAsync()
    {
        var count = GetSessionItems().Sum(item => item.Quantity);
        return Task.FromResult(count);
    }

    public async Task AddItemAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be at least 1.");
        }

        var product = await GetAvailableProductAsync(productId, cancellationToken);

        var sessionItems = GetSessionItems();
        var existingItem = sessionItems.SingleOrDefault(item => item.ProductId == productId);
        var currentQuantity = existingItem?.Quantity ?? 0;
        var nextQuantity = currentQuantity + quantity;

        if (nextQuantity > product.StockQuantity)
        {
            throw new InvalidOperationException($"Only {product.StockQuantity} item(s) available for {product.Name}.");
        }

        if (existingItem is null)
        {
            sessionItems.Add(new CartSessionItem
            {
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            existingItem.Quantity = nextQuantity;
        }

        SaveSessionItems(sessionItems);
    }

    public async Task UpdateQuantityAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be at least 1.");
        }

        var product = await GetAvailableProductAsync(productId, cancellationToken);
        if (quantity > product.StockQuantity)
        {
            throw new InvalidOperationException($"Only {product.StockQuantity} item(s) available for {product.Name}.");
        }

        var sessionItems = GetSessionItems();
        var existingItem = sessionItems.SingleOrDefault(item => item.ProductId == productId);
        if (existingItem is null)
        {
            throw new InvalidOperationException("Cart item not found.");
        }

        existingItem.Quantity = quantity;
        SaveSessionItems(sessionItems);
    }

    public Task RemoveItemAsync(int productId)
    {
        var sessionItems = GetSessionItems();
        var removed = sessionItems.RemoveAll(item => item.ProductId == productId);
        if (removed > 0)
        {
            SaveSessionItems(sessionItems);
        }

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        SaveSessionItems([]);
        return Task.CompletedTask;
    }

    private async Task<Product> GetAvailableProductAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == productId && item.Status == ProductStatus.Active, cancellationToken);

        if (product is null)
        {
            throw new InvalidOperationException("Product is no longer available.");
        }

        if (product.StockQuantity <= 0)
        {
            throw new InvalidOperationException("This product is currently out of stock.");
        }

        return product;
    }

    private List<CartSessionItem> GetSessionItems()
    {
        var session = GetSession();
        var json = session.GetString(CartSessionKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<CartSessionItem>>(json) ?? [];
    }

    private void SaveSessionItems(List<CartSessionItem> items)
    {
        var session = GetSession();

        if (items.Count == 0)
        {
            session.Remove(CartSessionKey);
            return;
        }

        session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
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

    private sealed class CartSessionItem
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
