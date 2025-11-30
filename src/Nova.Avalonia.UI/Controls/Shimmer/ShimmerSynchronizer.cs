using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Manages a single animation loop for all ShimmerViews.
/// Uses WeakReferences to prevent memory leaks if views are not detached properly.
/// </summary>
internal class ShimmerSynchronizer
{
    private readonly List<WeakReference<Shimmer>> _activeViews = new();
    private DispatcherTimer? _timer;
    private DateTime _startTime;
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(1.5);

    public void Add(Shimmer view)
    {
        CleanDeadReferences();
        _activeViews.Add(new WeakReference<Shimmer>(view));
        if (_activeViews.Count > 0)
            Start();
    }

    public void Remove(Shimmer view)
    {
        var toRemove = _activeViews.FirstOrDefault(w => w.TryGetTarget(out var target) && target == view);
        if (toRemove != null)
        {
            _activeViews.Remove(toRemove);
        }
        
        CleanDeadReferences();
        
        if (_activeViews.Count == 0)
            Stop();
    }

    private void CleanDeadReferences()
    {
        _activeViews.RemoveAll(w => !w.TryGetTarget(out _));
    }

    private void Start()
    {
        if (_timer != null) return;
        
        _startTime = DateTime.Now;
        _timer = new DispatcherTimer { Interval = Interval };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void Stop()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var elapsed = (DateTime.Now - _startTime).TotalSeconds;
        var progress = (elapsed % Duration.TotalSeconds) / Duration.TotalSeconds;

        bool hasActive = false;
        foreach (var weakRef in _activeViews)
        {
            if (weakRef.TryGetTarget(out var view))
            {
                view.UpdateOffset(progress);
                hasActive = true;
            }
        }

        // If all views died without removal, stop timer
        if (!hasActive && _activeViews.Count > 0)
        {
            CleanDeadReferences();
            if (_activeViews.Count == 0) Stop();
        }
    }
}
