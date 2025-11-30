namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Display mode for the Avatar control.
/// </summary>
public enum AvatarDisplayMode
{
    /// <summary>Display image if available, otherwise initials, otherwise icon.</summary>
    Auto,
    /// <summary>Display only image.</summary>
    Image,
    /// <summary>Display only initials from name.</summary>
    Initials,
    /// <summary>Display only icon.</summary>
    Icon,
    /// <summary>Display custom content.</summary>
    Content
}