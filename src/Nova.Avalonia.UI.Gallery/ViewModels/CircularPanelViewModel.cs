using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class CircularPanelViewModel : PageViewModel
{
    [ObservableProperty]
    private double _radius = 100;

    [ObservableProperty]
    private double _startAngle = 0;

    [ObservableProperty]
    private double _angleStep = 0; // 0 = auto

    [ObservableProperty]
    private bool _isClockwise = true;

    // Orientation derived from IsClockwise
    [ObservableProperty]
    private CircularOrientation _orientation = CircularOrientation.Clockwise;

    partial void OnIsClockwiseChanged(bool value)
    {
        Orientation = value ? CircularOrientation.Clockwise : CircularOrientation.CounterClockwise;
    }

    public ObservableCollection<CircularItem> Items { get; } = new();

    public CircularPanelViewModel() : base("CircularPanel")
    {
        // Initialize with sample items
        var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E9" };
        for (int i = 0; i < 8; i++)
        {
            Items.Add(new CircularItem($"Item {i + 1}", colors[i % colors.Length]));
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E9" };
        Items.Add(new CircularItem($"Item {Items.Count + 1}", colors[Items.Count % colors.Length]));
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
            Items.RemoveAt(Items.Count - 1);
    }
}

public record CircularItem(string Title, string ColorHex)
{
    public IBrush Brush => new SolidColorBrush(Color.Parse(ColorHex));
}
