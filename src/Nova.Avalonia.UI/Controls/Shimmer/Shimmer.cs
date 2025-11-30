using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A content-aware skeleton loading control.
/// Uses immediate mode rendering to draw a "shimmer" effect over the content layout.
/// Automatically detects text, shapes, and buttons to generate matching placeholders.
/// Includes A11y support and a synchronized animation clock to prevent visual clutter.
/// </summary>
public class Shimmer : ContentControl
{
    private const string DefaultAutomationName = "Loading content";

    // Shared synchronizer using WeakReferences to prevent memory leaks
    private static readonly ShimmerSynchronizer _synchronizer = new();

    private ContentPresenter? _contentPresenter;
    private GeometryGroup? _cachedGeometry;
    private IGradientBrush? _renderBrush;

    /// <summary>
    /// Defines whether the shimmer effect is active. 
    /// When true, content is hidden, interaction is disabled, and the skeleton is drawn.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Shimmer, bool>(nameof(IsLoading), defaultValue: true);

    /// <summary>
    /// The text announced by screen readers when the control is in the loading state.
    /// </summary>
    public static readonly StyledProperty<string> LoadingTextProperty =
        AvaloniaProperty.Register<Shimmer, string>(nameof(LoadingText), defaultValue: "Loading content");

    /// <summary>
    /// The brush used for the moving gradient highlight.
    /// Use this with DynamicResource to support theme switching.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HighlightBrushProperty =
        AvaloniaProperty.Register<Shimmer, IBrush?>(nameof(HighlightBrush));

    /// <summary>
    /// The opacity of the shimmer wave. 
    /// </summary>
    public static readonly StyledProperty<double> ShimmerOpacityProperty =
        AvaloniaProperty.Register<Shimmer, double>(nameof(ShimmerOpacity), defaultValue: 0.5);

    /// <summary>
    /// The angle of the shimmer gradient in degrees. 
    /// </summary>
    public static readonly StyledProperty<double> ShimmerAngleProperty =
        AvaloniaProperty.Register<Shimmer, double>(nameof(ShimmerAngle), defaultValue: 0.0);

    private double _synchronizedOffset;

    /// <inheritdoc cref="IsLoadingProperty"/>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <inheritdoc cref="LoadingTextProperty"/>
    public string LoadingText
    {
        get => GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    /// <summary>
    /// Adjusts the control type for accessibility tools.
    /// </summary>
    public AutomationControlType AutomationControlType { get; set; } = AutomationControlType.ProgressBar;

    /// <inheritdoc cref="HighlightBrushProperty"/>
    public IBrush? HighlightBrush
    {
        get => GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    /// <inheritdoc cref="ShimmerOpacityProperty"/>
    public double ShimmerOpacity
    {
        get => GetValue(ShimmerOpacityProperty);
        set => SetValue(ShimmerOpacityProperty, value);
    }

    /// <inheritdoc cref="ShimmerAngleProperty"/>
    public double ShimmerAngle
    {
        get => GetValue(ShimmerAngleProperty);
        set => SetValue(ShimmerAngleProperty, value);
    }

    static Shimmer()
    {
        AffectsRender<Shimmer>(
            IsLoadingProperty, 
            HighlightBrushProperty, 
            ShimmerOpacityProperty,
            ShimmerAngleProperty,
            BackgroundProperty
        );
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        UpdateVisualState();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (IsLoading)
        {
            _synchronizer.Add(this);
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ShimmerAutomationPeer(this);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _synchronizer.Remove(this);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsLoadingProperty)
        {
            UpdateVisualState();
            if (IsLoading) _synchronizer.Add(this);
            else _synchronizer.Remove(this);
        }
        else if (change.Property == LoadingTextProperty && IsLoading)
        {
            AutomationProperties.SetName(this, LoadingText);
        }
        else if (change.Property == BoundsProperty && IsLoading)
        {
            InvalidateGeometry();
        }
        else if (change.Property == HighlightBrushProperty)
        {
            _renderBrush = null;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Called by the static Synchronizer every frame to update the offset.
    /// </summary>
    internal void UpdateOffset(double offset)
    {
        _synchronizedOffset = offset;
        InvalidateVisual(); 
    }

    private void UpdateVisualState()
    {
        if (_contentPresenter == null) return;

        _contentPresenter.Opacity = IsLoading ? 0 : 1;
        _contentPresenter.IsHitTestVisible = !IsLoading;

        if (IsLoading)
        {
            AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);
            if (string.IsNullOrEmpty(AutomationProperties.GetName(this)))
            {
                AutomationProperties.SetName(this, string.IsNullOrWhiteSpace(LoadingText) ? DefaultAutomationName : LoadingText);
            }
        }
        else
        {
            AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Off);
        }

        PseudoClasses.Set(":loading", IsLoading);

        if (IsLoading)
        {
            Dispatcher.UIThread.Post(InvalidateGeometry, DispatcherPriority.Input);
        }
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!IsLoading || _cachedGeometry == null) return;
        
        // Use the standard Background property for the base skeleton
        if (Background != null)
        {
            context.DrawGeometry(Background, null, _cachedGeometry);
        }

        // Create the gradient brush dynamically from the HighlightBrush
        if (_renderBrush == null && HighlightBrush is ISolidColorBrush scb)
        {
            _renderBrush = new LinearGradientBrush
            {
                GradientStops = new GradientStops
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(scb.Color, 0.5),
                    new GradientStop(Colors.Transparent, 1)
                }
            };
        }

        if (_renderBrush is LinearGradientBrush linear)
        {
            if (Math.Abs(linear.Opacity - ShimmerOpacity) > 0.001)
                linear.Opacity = ShimmerOpacity;
            
            double angleRad = ShimmerAngle * Math.PI / 180.0;
            double r = 0.75; 
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);

            linear.StartPoint = new RelativePoint(0.5 - (cos * r), 0.5 - (sin * r), RelativeUnit.Relative);
            linear.EndPoint = new RelativePoint(0.5 + (cos * r), 0.5 + (sin * r), RelativeUnit.Relative);

            // Animation Steps
            double waveCenter = (_synchronizedOffset * 2.0) - 0.5;
            double waveWidth = 0.3; 

            linear.GradientStops[0].Offset = waveCenter - waveWidth;
            linear.GradientStops[1].Offset = waveCenter;
            linear.GradientStops[2].Offset = waveCenter + waveWidth;

            context.DrawGeometry(linear, null, _cachedGeometry);
        }
    }

    private void InvalidateGeometry()
    {
        if (!IsLoading || _contentPresenter?.Child == null) return;

        var group = new GeometryGroup { FillRule = FillRule.NonZero };
        TraverseVisualTree(_contentPresenter.Child, _contentPresenter, group);

        _cachedGeometry = group;
        InvalidateVisual(); 
    }

    private void TraverseVisualTree(Visual element, Visual relativeTo, GeometryGroup group)
    {
        if (element == null || !element.IsVisible) return;

        var bounds = element.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        bool isContent = false;
        double cornerRadius = 0;

        if (element is TextBlock || element is TextBox || element is AccessText)
        {
            isContent = true;
            cornerRadius = 2;
        }
        else if (element is Ellipse)
        {
            var transform = element.TranslatePoint(new Point(0, 0), relativeTo);
            if (transform.HasValue)
                group.Children.Add(new EllipseGeometry(new Rect(transform.Value.X, transform.Value.Y, bounds.Width, bounds.Height)));
            return;
        }
        else if (element is Rectangle r)
        {
            isContent = true;
            cornerRadius = Math.Max(r.RadiusX, r.RadiusY);
        }
        else if (element is Button)
        {
            isContent = true;
            cornerRadius = 4;
        }
        else if (element is Border b && !b.GetVisualChildren().Any())
        {
            isContent = true;
            cornerRadius = Math.Max(
                Math.Max(b.CornerRadius.TopLeft, b.CornerRadius.TopRight),
                Math.Max(b.CornerRadius.BottomLeft, b.CornerRadius.BottomRight));
        }
        else if (element is Image)
        {
            isContent = true;
            cornerRadius = 0;
        }

        if (isContent)
        {
            var transform = element.TranslatePoint(new Point(0, 0), relativeTo);
            if (transform.HasValue)
            {
                var rect = new Rect(transform.Value.X, transform.Value.Y, bounds.Width, bounds.Height);
                if (cornerRadius > 0)
                    group.Children.Add(new RectangleGeometry(rect, cornerRadius, cornerRadius));
                else
                    group.Children.Add(new RectangleGeometry(rect));
            }
        }

        if (!(element is Button || element is TextBox || element is TextBlock))
        {
            foreach (var child in element.GetVisualChildren())
                TraverseVisualTree(child, relativeTo, group);
        }
    }
}