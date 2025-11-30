using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Nova.Avalonia.UI.Controls;

public class AvatarStackPanel : Panel
{
    /// <summary>
    /// Percentage (0-1) of overlap between consecutive avatars; 0.25 means 25% overlap.
    /// </summary>
    public static readonly StyledProperty<double> OverlapProperty =
        AvaloniaProperty.Register<AvatarStackPanel, double>(nameof(Overlap), 0.25);

    /// <summary>
    /// Layout orientation for the stack (horizontal default).
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AvatarStackPanel, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>
    /// Percentage of overlap between children; values &lt; 0 produce gaps, &gt; 1 fully hides previous items.
    /// </summary>
    public double Overlap
    {
        get => GetValue(OverlapProperty);
        set => SetValue(OverlapProperty, value);
    }

    /// <summary>
    /// Direction of stacking (horizontal or vertical).
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    static AvatarStackPanel()
    {
        AffectsMeasure<AvatarStackPanel>(OverlapProperty, OrientationProperty);
        AffectsArrange<AvatarStackPanel>(OverlapProperty, OrientationProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children;
        if (children.Count == 0)
            return new Size(0, 0);

        double totalWidth = 0;
        double totalHeight = 0;
        double maxWidth = 0;
        double maxHeight = 0;

        // Measure all children to know their desired sizes
        foreach (var child in children)
        {
            child.Measure(availableSize);
            maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        // Calculate the cumulative size based on Overlap
        if (Orientation == Orientation.Horizontal)
        {
            // First item takes full width, subsequent items take (Width * (1-Overlap))
            // We approximate that all items are roughly the same size (maxWidth) for the container calculation
            // to ensure we have enough space.
            var visibleItemWidth = maxWidth * (1.0 - Overlap);
            totalWidth = maxWidth + (visibleItemWidth * (children.Count - 1));
            totalHeight = maxHeight;
        }
        else // Vertical
        {
            var visibleItemHeight = maxHeight * (1.0 - Overlap);
            totalHeight = maxHeight + (visibleItemHeight * (children.Count - 1));
            totalWidth = maxWidth;
        }

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        double currentPosition = 0;

        // Loop through children
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];

            // Enforce Z-Index Stacking order (First item on top)
            // We set this directly on the child control during arrange
            child.ZIndex = children.Count - i;

            if (Orientation == Orientation.Horizontal)
            {
                // We use DesiredSize so we don't stretch the child
                child.Arrange(new Rect(currentPosition, 0, child.DesiredSize.Width, finalSize.Height));

                // Increment position for next item, subtracting the overlap
                currentPosition += child.DesiredSize.Width * (1.0 - Overlap);
            }
            else // Vertical
            {
                // For vertical, we center horizontally if the panel is wider than the child
                // But usually we align left. Let's stick to standard flow.

                child.Arrange(new Rect(0, currentPosition, finalSize.Width, child.DesiredSize.Height));

                currentPosition += child.DesiredSize.Height * (1.0 - Overlap);
            }
        }

        return finalSize;
    }
}
