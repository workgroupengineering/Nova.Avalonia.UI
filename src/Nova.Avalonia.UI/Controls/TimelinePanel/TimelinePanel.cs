using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements in a timeline pattern with spacing for connectors.
/// </summary>
public class TimelinePanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<TimelineOrientation> OrientationProperty =
        AvaloniaProperty.Register<TimelinePanel, TimelineOrientation>(nameof(Orientation), TimelineOrientation.Vertical);

    /// <summary>
    /// Defines the <see cref="ItemSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<TimelinePanel, double>(nameof(ItemSpacing), 20);

    /// <summary>
    /// Defines the <see cref="ConnectorWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ConnectorWidthProperty =
        AvaloniaProperty.Register<TimelinePanel, double>(nameof(ConnectorWidth), 40);

    /// <summary>
    /// Defines the <see cref="AlternateItems"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AlternateItemsProperty =
        AvaloniaProperty.Register<TimelinePanel, bool>(nameof(AlternateItems), false);

    /// <summary>
    /// Gets or sets the orientation of the timeline.
    /// </summary>
    public TimelineOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between items.
    /// </summary>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the width reserved for the connector line/markers.
    /// </summary>
    public double ConnectorWidth
    {
        get => GetValue(ConnectorWidthProperty);
        set => SetValue(ConnectorWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets whether items alternate sides (left/right for vertical, top/bottom for horizontal).
    /// </summary>
    public bool AlternateItems
    {
        get => GetValue(AlternateItemsProperty);
        set => SetValue(AlternateItemsProperty, value);
    }

    static TimelinePanel()
    {
        AffectsMeasure<TimelinePanel>(OrientationProperty, ItemSpacingProperty, ConnectorWidthProperty, AlternateItemsProperty);
        AffectsArrange<TimelinePanel>(OrientationProperty, ItemSpacingProperty, ConnectorWidthProperty, AlternateItemsProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return new Size(0, 0);

        double totalPrimary = 0;
        double maxSecondary = 0;

        foreach (var child in visibleChildren)
        {
            child.Measure(Size.Infinity);

            if (Orientation == TimelineOrientation.Vertical)
            {
                totalPrimary += child.DesiredSize.Height;
                maxSecondary = Math.Max(maxSecondary, child.DesiredSize.Width);
            }
            else
            {
                totalPrimary += child.DesiredSize.Width;
                maxSecondary = Math.Max(maxSecondary, child.DesiredSize.Height);
            }
        }

        // Add spacing
        totalPrimary += (visibleChildren.Count - 1) * ItemSpacing;

        if (Orientation == TimelineOrientation.Vertical)
        {
            // Width = connector + content (or 2x content if alternating)
            double width = AlternateItems ? maxSecondary * 2 + ConnectorWidth : maxSecondary + ConnectorWidth;
            return new Size(width, totalPrimary);
        }
        else
        {
            double height = AlternateItems ? maxSecondary * 2 + ConnectorWidth : maxSecondary + ConnectorWidth;
            return new Size(totalPrimary, height);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return finalSize;

        double currentPosition = 0;
        int index = 0;

        foreach (var child in visibleChildren)
        {
            double x, y, width, height;

            if (Orientation == TimelineOrientation.Vertical)
            {
                height = child.DesiredSize.Height;
                width = child.DesiredSize.Width;
                y = currentPosition;

                if (AlternateItems)
                {
                    // Alternate left/right of center connector
                    double sideWidth = (finalSize.Width - ConnectorWidth) / 2;
                    if (index % 2 == 0)
                    {
                        // Left side
                        x = sideWidth - width;
                    }
                    else
                    {
                        // Right side
                        x = sideWidth + ConnectorWidth;
                    }
                }
                else
                {
                    // All items on right of connector
                    x = ConnectorWidth;
                }

                currentPosition += height + ItemSpacing;
            }
            else
            {
                width = child.DesiredSize.Width;
                height = child.DesiredSize.Height;
                x = currentPosition;

                if (AlternateItems)
                {
                    double sideHeight = (finalSize.Height - ConnectorWidth) / 2;
                    if (index % 2 == 0)
                    {
                        // Top side
                        y = sideHeight - height;
                    }
                    else
                    {
                        // Bottom side
                        y = sideHeight + ConnectorWidth;
                    }
                }
                else
                {
                    // All items below connector
                    y = ConnectorWidth;
                }

                currentPosition += width + ItemSpacing;
            }

            child.Arrange(new Rect(x, y, width, height));
            index++;
        }

        return finalSize;
    }
}
