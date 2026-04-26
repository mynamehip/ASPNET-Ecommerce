using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ASPNET_Ecommerce.Infrastructure.ModelBinding;

public sealed class FlexibleDecimalModelBinderProvider : IModelBinderProvider
{
    private static readonly IModelBinder Binder = new FlexibleDecimalModelBinder();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.UnderlyingOrModelType == typeof(decimal)
            ? Binder
            : null;
    }
}