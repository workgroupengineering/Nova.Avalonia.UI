using CommunityToolkit.Mvvm.ComponentModel;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public sealed partial class ShimmerViewModel : PageViewModel
{
    public ShimmerViewModel() : base("Shimmer")
    {
    }

    [ObservableProperty] private bool _isLoading = true;
}
