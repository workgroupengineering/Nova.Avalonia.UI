using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Automation peer for the <see cref="Shimmer"/> control, exposed as a loading indicator.
/// </summary>
public sealed class ShimmerAutomationPeer : ControlAutomationPeer
{
    private readonly Shimmer _owner;

    /// <summary>
    /// Creates a peer for the provided shimmer instance.
    /// </summary>
    public ShimmerAutomationPeer(Shimmer owner) : base(owner)
    {
        _owner = owner;
    }

    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = string.IsNullOrWhiteSpace(_owner.LoadingText)
                ? "Loading"
                : _owner.LoadingText;
        }
        return name;
    }

    protected override AutomationControlType GetAutomationControlTypeCore() =>
        _owner.AutomationControlType;

    protected override bool IsContentElementCore() => false;
}
