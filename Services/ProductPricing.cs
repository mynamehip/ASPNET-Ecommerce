using ASPNET_Ecommerce.Models.Catalog;

namespace ASPNET_Ecommerce.Services;

public static class ProductPricing
{
    public static bool HasActiveDiscount(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return HasActiveDiscount(product.IsDiscountActive, product.DiscountPercentage);
    }

    public static bool HasActiveDiscount(bool isDiscountActive, decimal? discountPercentage)
    {
        return isDiscountActive && discountPercentage.HasValue && discountPercentage.Value > 0;
    }

    public static decimal GetEffectivePrice(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return GetEffectivePrice(product.Price, product.IsDiscountActive, product.DiscountPercentage);
    }

    public static decimal GetEffectivePrice(decimal price, bool isDiscountActive, decimal? discountPercentage)
    {
        if (!HasActiveDiscount(isDiscountActive, discountPercentage))
        {
            return price;
        }

        var effectivePrice = price * (1 - (discountPercentage!.Value / 100m));
        return decimal.Round(effectivePrice, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal GetDiscountAmount(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return decimal.Round(product.Price - GetEffectivePrice(product), 2, MidpointRounding.AwayFromZero);
    }
}
