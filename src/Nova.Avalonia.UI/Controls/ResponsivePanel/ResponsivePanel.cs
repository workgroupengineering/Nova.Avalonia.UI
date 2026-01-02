using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines the size classes (breakpoints) for responsive layouts.
/// </summary>
[Flags]
public enum ResponsiveBreakpoint
{
    None = 0,
    Narrow = 1,
    Normal = 2,
    Wide = 4,
    All = Narrow | Normal | Wide
}

/// <summary>
/// A panel that selectively shows its children based on the current width breakpoint.
/// </summary>
public class ResponsivePanel : Panel
{
    // Define standard Material Design breakpoints
    public static readonly StyledProperty<double> NarrowBreakpointProperty =
        AvaloniaProperty.Register<ResponsivePanel, double>(nameof(NarrowBreakpoint), 600);

    public static readonly StyledProperty<double> WideBreakpointProperty =
        AvaloniaProperty.Register<ResponsivePanel, double>(nameof(WideBreakpoint), 900);

    // Attached Property: Condition
    public static readonly AttachedProperty<ResponsiveBreakpoint> ConditionProperty =
        AvaloniaProperty.RegisterAttached<ResponsivePanel, Control, ResponsiveBreakpoint>("Condition", ResponsiveBreakpoint.All);

    // Transition Property
    public static readonly StyledProperty<IPageTransition?> TransitionProperty =
        AvaloniaProperty.Register<ResponsivePanel, IPageTransition?>(nameof(Transition));

    public IPageTransition? Transition
    {
        get => GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    private ResponsiveBreakpoint _lastBreakpoint = ResponsiveBreakpoint.None;
    private CancellationTokenSource? _transitionCts;

    public double NarrowBreakpoint
    {
        get => GetValue(NarrowBreakpointProperty);
        set => SetValue(NarrowBreakpointProperty, value);
    }

    public double WideBreakpoint
    {
        get => GetValue(WideBreakpointProperty);
        set => SetValue(WideBreakpointProperty, value);
    }

    public static ResponsiveBreakpoint GetCondition(Control element) => element.GetValue(ConditionProperty);
    public static void SetCondition(Control element, ResponsiveBreakpoint value) => element.SetValue(ConditionProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        var currentBreakpoint = GetCurrentBreakpoint(availableSize.Width);
        
        // If breakpoint changed, handle transition
        // If breakpoint changed, handle transition
        if (currentBreakpoint != _lastBreakpoint)
        {
            var isFirstLoad = _lastBreakpoint == ResponsiveBreakpoint.None;
            UpdateVisibility(currentBreakpoint, !isFirstLoad);
            _lastBreakpoint = currentBreakpoint;
        }

        var maxDesiredWidth = 0.0;
        var maxDesiredHeight = 0.0;

        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                child.Measure(availableSize);
                maxDesiredWidth = Math.Max(maxDesiredWidth, child.DesiredSize.Width);
                maxDesiredHeight = Math.Max(maxDesiredHeight, child.DesiredSize.Height);
            }
        }

        return new Size(maxDesiredWidth, maxDesiredHeight);
    }

    private async void UpdateVisibility(ResponsiveBreakpoint newBreakpoint, bool animate = true)
    {
        var transition = Transition;
        
        // Identify Incoming/Outgoing
        var incoming = new List<Control>();
        var outgoing = new List<Control>();

        foreach (var child in Children)
        {
            var condition = GetCondition(child);
            var matchesNew = condition.HasFlag(newBreakpoint);
            var matchesOld = condition.HasFlag(_lastBreakpoint);

            // If _lastBreakpoint is None (First Load), we consider nothing as "Old Matching" 
            // to avoid confusing logic. Usually matchesOld would be true for everything if None=0.
            if (_lastBreakpoint == ResponsiveBreakpoint.None) matchesOld = false;

            if (matchesNew && !matchesOld) incoming.Add(child);
            else if (!matchesNew && matchesOld) outgoing.Add(child);
            // Else: Persisting or remaining hidden
        }

        // Cancel previous transition
        _transitionCts?.Cancel();
        _transitionCts = new CancellationTokenSource();
        var token = _transitionCts.Token;

        if (animate && transition != null && (incoming.Count > 0 || outgoing.Count > 0))
        {
            // Prepare for animation
            foreach (var view in incoming) view.IsVisible = true;
            // Outgoing are already visible

            try
            {
                // We simplify by treating the first matching items as the "Page" for the transition
                // Ideally we'd wrap them, but for now we animate them individually or just the first primary one.
                // Standard PageTransition expects Single From/To.
                // If we have multiple children for a condition, this is tricky. 
                // We will iterate and trigger generic fade for all? 
                // Or just assume standard usage of 1 child per view.
                
                var tasks = new List<Task>();
                
                // For simplicity in this v1, if there are multiple, we create a composite task?
                // Actually IPageTransition only accepts one From and one To.
                
                Control? from = outgoing.FirstOrDefault();
                Control? to = incoming.FirstOrDefault();
                
                await transition.Start(from, to, true, token);
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                   // Cleanup
                   foreach (var view in outgoing) view.IsVisible = false;
                }
            }
        }
        else
        {
            // Instant Toggle
            foreach (var child in Children)
            {
                var condition = GetCondition(child);
                child.IsVisible = condition.HasFlag(newBreakpoint);
            }
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var currentBreakpoint = GetCurrentBreakpoint(finalSize.Width);

        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                child.Arrange(new Rect(finalSize));
            }
        }

        return finalSize;
    }

    private ResponsiveBreakpoint GetCurrentBreakpoint(double width)
    {
        if (double.IsInfinity(width)) return ResponsiveBreakpoint.Wide; // Fallback for infinite width

        if (width < NarrowBreakpoint) return ResponsiveBreakpoint.Narrow;
        if (width < WideBreakpoint) return ResponsiveBreakpoint.Normal;
        return ResponsiveBreakpoint.Wide;
    }
}
