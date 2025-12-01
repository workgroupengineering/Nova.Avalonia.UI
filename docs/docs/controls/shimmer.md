---
title: Shimmer
description: Use the Shimmer control to render skeleton placeholders while data is loading.
ms.date: 2025-12-01
---

# Shimmer

The `Shimmer` control shows a lightweight skeleton while your content is loading. It inspects the visual tree beneath it to draw shapes that match text, images, and buttons, then animates a gradient sweep over the placeholders.

## Add a Shimmer placeholder

Wrap the content that loads asynchronously in a `Shimmer`. Toggle `IsLoading` to switch between the placeholder and the real content.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <nova:Shimmer IsLoading="True" LoadingText="Loading profile">
        <StackPanel Spacing="8">
            <TextBlock FontSize="20" Text="Profile" />
            <Border Height="160" CornerRadius="12" Background="#202020" />
            <Button Content="Refresh" Width="120" />
        </StackPanel>
    </nova:Shimmer>
</UserControl>
```

When `IsLoading` is `True`, Shimmer disables hit testing on the child content and announces the loading state to screen readers.

## Customize the effect

Use the following properties to align the effect with your theme:

- `HighlightBrush` sets the moving gradient. Bind it to a `DynamicResource` for theme switching.
- `ShimmerOpacity` adjusts the overlay opacity. The default is `0.5`.
- `ShimmerAngle` sets the gradient angle in degrees.
- `LoadingText` defines the automation name announced while loading.

```xaml
<nova:Shimmer IsLoading="True"
              HighlightBrush="{DynamicResource AccentGradient}"
              ShimmerOpacity="0.35"
              ShimmerAngle="12"
              LoadingText="Loading dashboard cards" />
```

## Show loaded content

Set `IsLoading` to `False` when your data is ready. The child content becomes visible and interactive, and the automation name is cleared.

```csharp
// ViewModel
public bool IsBusy { get; set; }
```

```xaml
<nova:Shimmer IsLoading="{Binding IsBusy}">
    <ItemsControl Items="{Binding Orders}" />
</nova:Shimmer>
```