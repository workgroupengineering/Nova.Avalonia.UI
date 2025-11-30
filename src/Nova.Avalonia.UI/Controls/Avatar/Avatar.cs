using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Extensible Avatar control that displays user images, initials, icons, or custom content.
/// </summary>
public class Avatar : TemplatedControl
{
    private Border? _container;
    private ContentPresenter? _contentPresenter;
    private TextBlock? _initialsText;
    private ContentControl? _iconPresenter;
    private Image? _imagePresenter;
    private Border? _statusIndicator;

    /// <summary>
    /// Defines the <see cref="DisplayName"/> property.
    /// </summary>
    public static readonly StyledProperty<string> DisplayNameProperty =
        AvaloniaProperty.Register<Avatar, string>(nameof(DisplayName), string.Empty);

    /// <summary>
    /// Defines the <see cref="ImageSource"/> property.
    /// </summary>
    public static readonly StyledProperty<Bitmap?> ImageSourceProperty =
        AvaloniaProperty.Register<Avatar, Bitmap?>(nameof(ImageSource));

    /// <summary>
    /// Defines the <see cref="Icon"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<Avatar, object?>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Avatar, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="DisplayMode"/> property.
    /// </summary>
    public static readonly StyledProperty<AvatarDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<Avatar, AvatarDisplayMode>(nameof(DisplayMode), AvatarDisplayMode.Auto);

    /// <summary>
    /// Defines the <see cref="Shape"/> property.
    /// </summary>
    public static readonly StyledProperty<AvatarShape> ShapeProperty =
        AvaloniaProperty.Register<Avatar, AvatarShape>(nameof(Shape), AvatarShape.Circle);

    /// <summary>
    /// Defines the <see cref="Size"/> property.
    /// </summary>
    public static readonly StyledProperty<AvatarSize> SizeProperty =
        AvaloniaProperty.Register<Avatar, AvatarSize>(nameof(Size), AvatarSize.Medium);

    /// <summary>
    /// Defines the <see cref="CustomSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CustomSizeProperty =
        AvaloniaProperty.Register<Avatar, double>(nameof(CustomSize), 48.0);

    /// <summary>
    /// Defines the <see cref="BackgroundColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BackgroundColorProperty =
        AvaloniaProperty.Register<Avatar, IBrush>(nameof(BackgroundColor), new SolidColorBrush(Color.Parse("#3B82F6")));

    /// <summary>
    /// Defines the <see cref="AutoGenerateBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoGenerateBackgroundProperty =
        AvaloniaProperty.Register<Avatar, bool>(nameof(AutoGenerateBackground), true);

    /// <summary>
    /// Defines the <see cref="ForegroundColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> ForegroundColorProperty =
        AvaloniaProperty.Register<Avatar, IBrush>(nameof(ForegroundColor), Brushes.White);

    /// <summary>
    /// Defines the <see cref="Status"/> property.
    /// </summary>
    public static readonly StyledProperty<AvatarStatus> StatusProperty =
        AvaloniaProperty.Register<Avatar, AvatarStatus>(nameof(Status), AvatarStatus.None);

    /// <summary>
    /// Defines the <see cref="StatusColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StatusColorProperty =
        AvaloniaProperty.Register<Avatar, IBrush?>(nameof(StatusColor), null);

    /// <summary>
    /// Defines the <see cref="Initials"/> property.
    /// </summary>
    public static readonly StyledProperty<string> InitialsProperty =
        AvaloniaProperty.Register<Avatar, string>(nameof(Initials), string.Empty);

    /// <summary>
    /// Defines the <see cref="MaxInitials"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxInitialsProperty =
        AvaloniaProperty.Register<Avatar, int>(nameof(MaxInitials), 2);

    /// <summary>
    /// Defines the <see cref="ShowTooltip"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowTooltipProperty =
        AvaloniaProperty.Register<Avatar, bool>(nameof(ShowTooltip), true);

    /// <summary>
    /// Gets or sets the person's display name used for generating initials and tooltip.
    /// </summary>
    public string DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the image source for the avatar.
    /// </summary>
    public Bitmap? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon content (can be PathIcon, SymbolIcon, or any content).
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets custom content to display in the avatar.
    /// </summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the display mode determining what content to show.
    /// </summary>
    public AvatarDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the shape of the avatar.
    /// </summary>
    public AvatarShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// Gets or sets the predefined size of the avatar.
    /// </summary>
    public AvatarSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the custom size when Size is set to Custom.
    /// </summary>
    public double CustomSize
    {
        get => GetValue(CustomSizeProperty);
        set => SetValue(CustomSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the background color of the avatar.
    /// </summary>
    public IBrush BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to automatically generate background color from name.
    /// </summary>
    public bool AutoGenerateBackground
    {
        get => GetValue(AutoGenerateBackgroundProperty);
        set => SetValue(AutoGenerateBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground color for text and icons.
    /// </summary>
    public IBrush ForegroundColor
    {
        get => GetValue(ForegroundColorProperty);
        set => SetValue(ForegroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the status indicator.
    /// </summary>
    public AvatarStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of the status indicator. If null, uses default color for status.
    /// </summary>
    public IBrush? StatusColor
    {
        get => GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    /// <summary>
    /// Gets or sets custom initials (overrides auto-generated initials from Name).
    /// </summary>
    public string Initials
    {
        get => GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of initials to display.
    /// </summary>
    public int MaxInitials
    {
        get => GetValue(MaxInitialsProperty);
        set => SetValue(MaxInitialsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show tooltip with full name.
    /// </summary>
    public bool ShowTooltip
    {
        get => GetValue(ShowTooltipProperty);
        set => SetValue(ShowTooltipProperty, value);
    }

    static Avatar()
    {
        DisplayNameProperty.Changed.AddClassHandler<Avatar>((x, e) => x.OnContentChanged());
        ImageSourceProperty.Changed.AddClassHandler<Avatar>((x, e) => x.OnContentChanged());
        IconProperty.Changed.AddClassHandler<Avatar>((x, e) => x.OnContentChanged());
        ContentProperty.Changed.AddClassHandler<Avatar>((x, e) => x.OnContentChanged());
        DisplayModeProperty.Changed.AddClassHandler<Avatar>((x, e) => x.OnContentChanged());
        ShapeProperty.Changed.AddClassHandler<Avatar>((x, e) => x.UpdateShape());
        SizeProperty.Changed.AddClassHandler<Avatar>((x, e) => x.UpdateSize());
        CustomSizeProperty.Changed.AddClassHandler<Avatar>((x, e) => x.UpdateSize());
        BackgroundColorProperty.Changed.AddClassHandler<Avatar>((x, e) => x.UpdateBackground());
        AutoGenerateBackgroundProperty.Changed.AddClassHandler<Avatar>((x, e) => x.UpdateBackground());
        InitialsProperty.Changed.AddClassHandler<Avatar>((x, e) => x.OnContentChanged());
        StatusProperty.Changed.AddClassHandler<Avatar>((x, e) => x.UpdateStatus());
        StatusColorProperty.Changed.AddClassHandler<Avatar>((x, e) => x.UpdateStatus());
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _container = e.NameScope.Find<Border>("PART_Container");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_Content");
        _initialsText = e.NameScope.Find<TextBlock>("PART_Initials");
        _iconPresenter = e.NameScope.Find<ContentControl>("PART_Icon");
        _imagePresenter = e.NameScope.Find<Image>("PART_Image");
        _statusIndicator = e.NameScope.Find<Border>("PART_StatusIndicator");

        UpdateShape();
        UpdateSize();
        UpdateBackground();
        UpdateTooltip();
        UpdateContent();
        UpdateStatus();
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new AvatarAutomationPeer(this);
    }

    private void OnContentChanged()
    {
        UpdateBackground();
        UpdateTooltip();
        UpdateContent();
    }

    private void UpdateContent()
    {
        if (_contentPresenter == null || _initialsText == null ||
            _iconPresenter == null || _imagePresenter == null)
            return;

        _contentPresenter.IsVisible = false;
        _initialsText.IsVisible = false;
        _iconPresenter.IsVisible = false;
        _imagePresenter.IsVisible = false;

        var mode = DetermineEffectiveDisplayMode();

        switch (mode)
        {
            case AvatarDisplayMode.Image:
                _imagePresenter.IsVisible = true;
                _imagePresenter.Source = ImageSource;
                break;

            case AvatarDisplayMode.Initials:
                _initialsText.IsVisible = true;
                _initialsText.Text = GetInitials();
                _initialsText.FontSize = GetFontSize();
                break;

            case AvatarDisplayMode.Icon:
                _iconPresenter.IsVisible = true;
                _iconPresenter.Content = Icon;
                break;

            case AvatarDisplayMode.Content:
                _contentPresenter.IsVisible = true;
                _contentPresenter.Content = Content;
                break;
        }
    }

    private AvatarDisplayMode DetermineEffectiveDisplayMode()
    {
        if (DisplayMode != AvatarDisplayMode.Auto)
            return DisplayMode;

        if (ImageSource != null)
            return AvatarDisplayMode.Image;

        if (!string.IsNullOrWhiteSpace(DisplayName) || !string.IsNullOrWhiteSpace(Initials))
            return AvatarDisplayMode.Initials;

        if (Icon != null)
            return AvatarDisplayMode.Icon;

        if (Content != null)
            return AvatarDisplayMode.Content;

        return AvatarDisplayMode.Initials;
    }

    private string GetInitials()
    {
        if (!string.IsNullOrWhiteSpace(Initials))
            return Initials.Substring(0, Math.Min(Initials.Length, MaxInitials)).ToUpper();

        if (string.IsNullOrWhiteSpace(DisplayName))
            return "?";

        var parts = DisplayName.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return "?";

        if (parts.Length == 1)
        {
            var name = parts[0];
            var count = Math.Min(MaxInitials, name.Length);
            return name.Substring(0, count).ToUpper();
        }

        var initials = string.Concat(parts.Take(MaxInitials).Select(p =>
            char.ToUpper(p[0], CultureInfo.InvariantCulture)));

        return initials;
    }

    private void UpdateShape()
    {
        if (_container == null)
            return;

        var size = GetActualSize();

        switch (Shape)
        {
            case AvatarShape.Circle:
                _container.CornerRadius = new CornerRadius(size / 2);
                break;
            case AvatarShape.Square:
                _container.CornerRadius = new CornerRadius(size * 0.15);
                break;
            case AvatarShape.Rectangle:
                _container.CornerRadius = new CornerRadius(0);
                break;
        }
    }

    private void UpdateSize()
    {
        var size = GetActualSize();
        Width = size;
        Height = size;

        UpdateShape();

        if (_initialsText != null)
        {
            _initialsText.FontSize = GetFontSize();
        }
    }

    private double GetActualSize()
    {
        return Size switch
        {
            AvatarSize.ExtraSmall => 24,
            AvatarSize.Small => 32,
            AvatarSize.Medium => 48,
            AvatarSize.Large => 64,
            AvatarSize.ExtraLarge => 96,
            AvatarSize.Custom => CustomSize,
            _ => 48
        };
    }

    private double GetFontSize()
    {
        var size = GetActualSize();
        return size * 0.4;
    }

    private void UpdateBackground()
    {
        if (_container == null)
            return;

        if (AutoGenerateBackground && !string.IsNullOrWhiteSpace(DisplayName))
        {
            _container.Background = GenerateBackgroundFromName(DisplayName);
        }
        else
        {
            _container.Background = BackgroundColor;
        }
    }

    private IBrush GenerateBackgroundFromName(string name)
    {
        var hash = name.GetHashCode();
        var hue = Math.Abs(hash % 360);
        var color = HslToRgb(hue, 0.65, 0.45);
        return new SolidColorBrush(color);
    }

    private Color HslToRgb(double h, double s, double l)
    {
        h = h / 360.0;
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double HueToRgb(double p, double q, double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
                if (t < 1.0 / 2.0) return q;
                if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
                return p;
            }

            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = HueToRgb(p, q, h + 1.0 / 3.0);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3.0);
        }

        return Color.FromRgb(
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255));
    }

    private void UpdateTooltip()
    {
        if (ShowTooltip && !string.IsNullOrWhiteSpace(DisplayName))
        {
            ToolTip.SetTip(this, DisplayName);
        }
        else
        {
            ToolTip.SetTip(this, null);
        }
    }

    private void UpdateStatus()
    {
        if (_statusIndicator == null)
            return;

        _statusIndicator.IsVisible = Status != AvatarStatus.None;

        if (Status != AvatarStatus.None)
        {
            _statusIndicator.Background = GetStatusColor();

            var size = GetActualSize();
            var indicatorSize = size * 0.25;
            _statusIndicator.Width = indicatorSize;
            _statusIndicator.Height = indicatorSize;
            _statusIndicator.CornerRadius = new CornerRadius(indicatorSize / 2);
        }
    }

    private IBrush GetStatusColor()
    {
        if (StatusColor != null)
            return StatusColor;

        return Status switch
        {
            AvatarStatus.Online => new SolidColorBrush(Color.Parse("#10B981")), // Green
            AvatarStatus.Offline => new SolidColorBrush(Color.Parse("#6B7280")), // Gray
            AvatarStatus.Away => new SolidColorBrush(Color.Parse("#F59E0B")), // Amber
            AvatarStatus.Busy => new SolidColorBrush(Color.Parse("#EF4444")), // Red
            AvatarStatus.DoNotDisturb => new SolidColorBrush(Color.Parse("#DC2626")), // Dark Red
            _ => Brushes.Transparent
        };
    }
}
    