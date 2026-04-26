namespace ASPNET_Ecommerce.Models.ViewModels.Admin;

public class SliderIndexViewModel
{
    public IReadOnlyList<SliderListItemViewModel> BannerItems { get; set; } = [];

    public IReadOnlyList<SliderListItemViewModel> SlideItems { get; set; } = [];
}
