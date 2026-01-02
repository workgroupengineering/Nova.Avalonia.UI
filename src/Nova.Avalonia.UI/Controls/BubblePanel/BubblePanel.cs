using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements using a circle packing algorithm.
/// Larger items are placed first and smaller items fill the gaps.
/// </summary>
public class BubblePanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Padding"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<BubblePanel, Thickness>(nameof(Padding));

    /// <summary>
    /// Defines the <see cref="ItemSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<BubblePanel, double>(nameof(ItemSpacing), 4);

    /// <summary>
    /// Defines the Size attached property for specifying relative size.
    /// </summary>
    public static readonly AttachedProperty<double> SizeProperty =
        AvaloniaProperty.RegisterAttached<BubblePanel, Control, double>("Size", 1.0);

    /// <summary>
    /// Gets or sets the padding around the panel content.
    /// </summary>
    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum spacing between bubbles.
    /// </summary>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// Gets the Size attached property value.
    /// </summary>
    public static double GetSize(Control element) => element.GetValue(SizeProperty);

    /// <summary>
    /// Sets the Size attached property value.
    /// </summary>
    public static void SetSize(Control element, double value) => element.SetValue(SizeProperty, value);

    static BubblePanel()
    {
        AffectsMeasure<BubblePanel>(PaddingProperty, ItemSpacingProperty);
        AffectsArrange<BubblePanel>(PaddingProperty, ItemSpacingProperty);
    }

    private class PlacedCircle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
        public Control? Child { get; set; }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return new Size(0, 0);

        foreach (var child in visibleChildren)
        {
            child.Measure(Size.Infinity);
        }

        // Estimate size based on children
        double totalArea = 0;
        foreach (var child in visibleChildren)
        {
            var size = GetSize(child);
            var radius = Math.Max(child.DesiredSize.Width, child.DesiredSize.Height) / 2 * size;
            totalArea += Math.PI * radius * radius;
        }

        // Estimate a square that could contain all circles (with some padding)
        double side = Math.Sqrt(totalArea) * 1.5;
        var padding = Padding;

        return new Size(side + padding.Left + padding.Right, side + padding.Top + padding.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return finalSize;

        var padding = Padding;
        double availableWidth = finalSize.Width - padding.Left - padding.Right;
        double availableHeight = finalSize.Height - padding.Top - padding.Bottom;
        double spacing = ItemSpacing;

        // Calculate radii and sort by size (largest first)
        var childData = visibleChildren.Select(child =>
        {
            var size = GetSize(child);
            var radius = Math.Max(child.DesiredSize.Width, child.DesiredSize.Height) / 2 * size;
            return new { Child = child, Radius = Math.Max(10, radius) };
        }).OrderByDescending(c => c.Radius).ToList();

        var placedCircles = new List<PlacedCircle>();
        double centerX = availableWidth / 2;
        double centerY = availableHeight / 2;

        foreach (var item in childData)
        {
            var position = FindBestPosition(placedCircles, item.Radius, centerX, centerY, availableWidth, availableHeight, spacing);

            placedCircles.Add(new PlacedCircle
            {
                X = position.x,
                Y = position.y,
                Radius = item.Radius,
                Child = item.Child
            });

            // Arrange the child
            double childSize = item.Radius * 2;
            double x = padding.Left + position.x - item.Radius;
            double y = padding.Top + position.y - item.Radius;

            item.Child.Arrange(new Rect(x, y, childSize, childSize));
        }

        return finalSize;
    }

    private (double x, double y) FindBestPosition(
        List<PlacedCircle> placed,
        double radius,
        double centerX,
        double centerY,
        double width,
        double height,
        double spacing)
    {
        if (placed.Count == 0)
        {
            return (centerX, centerY);
        }

        double bestX = centerX;
        double bestY = centerY;
        double bestDistance = double.MaxValue;
        bool foundValid = false;

        // Try positions around each placed circle with finer granularity
        foreach (var circle in placed)
        {
            // Try 24 positions around this circle (every 15 degrees)
            for (int angle = 0; angle < 360; angle += 15)
            {
                double rad = angle * Math.PI / 180;
                double distance = circle.Radius + radius + spacing;

                double candidateX = circle.X + distance * Math.Cos(rad);
                double candidateY = circle.Y + distance * Math.Sin(rad);

                // Check bounds
                if (candidateX - radius < 0 || candidateX + radius > width ||
                    candidateY - radius < 0 || candidateY + radius > height)
                    continue;

                // Check overlap with all placed circles
                bool overlaps = false;
                foreach (var other in placed)
                {
                    double dx = candidateX - other.X;
                    double dy = candidateY - other.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < radius + other.Radius + spacing)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    foundValid = true;
                    // Prefer positions closer to center
                    double distToCenter = Math.Sqrt(
                        Math.Pow(candidateX - centerX, 2) +
                        Math.Pow(candidateY - centerY, 2));

                    if (distToCenter < bestDistance)
                    {
                        bestDistance = distToCenter;
                        bestX = candidateX;
                        bestY = candidateY;
                    }
                }
            }
        }

        // If no valid position found, find any non-overlapping position by expanding outward
        if (!foundValid)
        {
            for (double r = radius + spacing; r < Math.Max(width, height); r += 20)
            {
                for (int angle = 0; angle < 360; angle += 30)
                {
                    double rad = angle * Math.PI / 180;
                    double candidateX = centerX + r * Math.Cos(rad);
                    double candidateY = centerY + r * Math.Sin(rad);

                    if (candidateX - radius < 0 || candidateX + radius > width ||
                        candidateY - radius < 0 || candidateY + radius > height)
                        continue;

                    bool overlaps = false;
                    foreach (var other in placed)
                    {
                        double dx = candidateX - other.X;
                        double dy = candidateY - other.Y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (dist < radius + other.Radius + spacing)
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        return (candidateX, candidateY);
                    }
                }
            }
        }

        return (bestX, bestY);
    }
}
