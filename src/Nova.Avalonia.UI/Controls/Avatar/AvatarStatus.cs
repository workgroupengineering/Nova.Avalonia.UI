namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Status indicator for Avatar (online status, notifications, etc.).
/// Default colors are provided by the Avatar control; set StatusColor to override.
/// </summary>
public enum AvatarStatus
{
    None,
    /// <summary>Contact is reachable and active.</summary>
    Online,
    /// <summary>Contact is not reachable.</summary>
    Offline,
    /// <summary>Contact is away/idle.</summary>
    Away,
    /// <summary>Contact is busy.</summary>
    Busy,
    /// <summary>Contact should not be disturbed.</summary>
    DoNotDisturb
}
