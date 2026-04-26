using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ASPNET_Ecommerce.Infrastructure.ModelBinding;

public sealed class FlexibleDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var rawValue = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            if (bindingContext.ModelMetadata.IsReferenceOrNullableType)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(
                        valueProviderResult.ToString()));
            }

            return Task.CompletedTask;
        }

        if (TryParseDecimal(rawValue.Trim(), out var parsedValue))
        {
            bindingContext.Result = ModelBindingResult.Success(parsedValue);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustBeANumberAccessor(
                valueProviderResult.ToString()));

        return Task.CompletedTask;
    }

    private static bool TryParseDecimal(string rawValue, out decimal parsedValue)
    {
        var cultures = new[]
        {
            CultureInfo.CurrentCulture,
            CultureInfo.CurrentUICulture,
            CultureInfo.InvariantCulture
        };

        foreach (var culture in cultures.DistinctBy(culture => culture.Name))
        {
            if (decimal.TryParse(rawValue, NumberStyles.Number, culture, out parsedValue))
            {
                return true;
            }
        }

        parsedValue = default;
        return false;
    }
}