using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements in a radial fan pattern emanating from a center point.
/// </summary>
public class RadialPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Radius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RadiusProperty =
        AvaloniaProperty.Register<RadialPanel, double>(nameof(Radius), 100);

    /// <summary>
    /// Defines the <see cref="StartAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<RadialPanel, double>(nameof(StartAngle), 0);

    /// <summary>
    /// Defines the <see cref="SweepAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<RadialPanel, double>(nameof(SweepAngle), 360);

    /// <summary>
    /// Defines the <see cref="ItemAngle"/> property for individual item rotation.
    /// </summary>
    public static readonly StyledProperty<double> ItemAngleProperty =
        AvaloniaProperty.Register<RadialPanel, double>(nameof(ItemAngle), 0);

    /// <summary>
    /// Defines the <see cref="RotateItems"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> RotateItemsProperty =
        AvaloniaProperty.Register<RadialPanel, bool>(nameof(RotateItems), true);

    /// <summary>
    /// Gets or sets the radius from center to item centers.
    /// </summary>
    public double Radius
    {
        get => GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting angle in degrees (0 = right, 90 = down).
    /// </summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the total angle span for items.
    /// </summary>
    public double SweepAngle
    {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets an additional rotation offset for all items.
    /// </summary>
    public double ItemAngle
    {
        get => GetValue(ItemAngleProperty);
        set => SetValue(ItemAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether items should rotate to face outward from center.
    /// </summary>
    public bool RotateItems
    {
        get => GetValue(RotateItemsProperty);
        set => SetValue(RotateItemsProperty, value);
    }

    static RadialPanel()
    {
        AffectsMeasure<RadialPanel>(RadiusProperty, StartAngleProperty, SweepAngleProperty, ItemAngleProperty, RotateItemsProperty);
        AffectsArrange<RadialPanel>(RadiusProperty, StartAngleProperty, SweepAngleProperty, ItemAngleProperty, RotateItemsProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return new Size(0, 0);

        double maxChildSize = 0;
        foreach (var child in visibleChildren)
        {
            child.Measure(Size.Infinity);
            maxChildSize = Math.Max(maxChildSize, Math.Max(child.DesiredSize.Width, child.DesiredSize.Height));
        }

        // Size is diameter plus largest child
        double diameter = Radius * 2 + maxChildSize;
        return new Size(diameter, diameter);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return finalSize;

        double centerX = finalSize.Width / 2;
        double centerY = finalSize.Height / 2;
        double radius = Radius;
        double startAngle = StartAngle;
        double sweepAngle = SweepAngle;

        // Calculate angle step
        double angleStep = visibleChildren.Count > 1 
            ? sweepAngle / (sweepAngle >= 360 ? visibleChildren.Count : visibleChildren.Count - 1)
            : 0;

        for (int i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            double angle = startAngle + (i * angleStep);
            double radians = angle * Math.PI / 180;

            // Calculate position
            double x = centerX + radius * Math.Cos(radians);
            double y = centerY + radius * Math.Sin(radians);

            // Center the child on its position
            double childWidth = child.DesiredSize.Width;
            double childHeight = child.DesiredSize.Height;

            // Apply rotation if enabled
            if (RotateItems)
            {
                child.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                child.RenderTransform = new global::Avalonia.Media.RotateTransform(angle + 90 + ItemAngle);
            }

            child.Arrange(new Rect(x - childWidth / 2, y - childHeight / 2, childWidth, childHeight));
        }

        return finalSize;
    }
}
