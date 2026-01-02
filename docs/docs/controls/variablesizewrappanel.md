---
title: VariableSizeWrapPanel
description: A panel that arranges varying-sized items in a wrapping grid, similar to the Windows Start Screen.
ms.date: 2026-01-01
---

# VariableSizeWrapPanel

The `VariableSizeWrapPanel` (formerly MetroPanel) lays out items based on a uniform unit size, but allows individual items to span multiple rows and columns. This layout style is famously used in the Windows 8/10 Start Screen "Metro" design.

## Basic Usage

Define the basic `ItemHeight` and `ItemWidth`. Items default to a 1x1 span of this unit size. Use `VariableSizeWrapPanel.RowSpan` and `VariableSizeWrapPanel.ColumnSpan` to make items larger.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:VariableSizeWrapPanel ItemHeight="100" ItemWidth="100" Orientation="Horizontal" MaximumRowsOrColumns="3">
        
        <!-- Standard 1x1 Item (100x100) -->
        <Border Background="Red" />
        
        <!-- Large 2x2 Item (200x200) -->
        <Border Background="Blue" 
                nova:VariableSizeWrapPanel.RowSpan="2" 
                nova:VariableSizeWrapPanel.ColumnSpan="2" />
        
        <!-- Wide 2x1 Item (200x100) -->
        <Border Background="Green" 
                nova:VariableSizeWrapPanel.ColumnSpan="2" />
                
    </nova:VariableSizeWrapPanel>
</UserControl>
```

## Important Considerations

- The panel fills available space in the `Orientation` direction until `MaximumRowsOrColumns` is reached (or space runs out), then wraps to the next line/column.
- **Item Order**: The panel tries to fit items in the "holes" left by larger items if the `Orientation` allows, but generally places items sequentially. Layout gaps may occur if items dimensions don't sum up perfectly to the line capacity.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ItemHeight` | `double` | `NaN` | The base height of a 1-unit block. |
| `ItemWidth` | `double` | `NaN` | The base width of a 1-unit block. |
| `Orientation` | `Orientation` | `Vertical` | The direction in which items are arranged before wrapping. |
| `MaximumRowsOrColumns` | `int` | `-1` | The maximum number of units in the non-wrapping dimension before forcing a wrap. |

## Attached Properties

| Property | Type | Description |
|----------|------|-------------|
| `RowSpan` | `int` | How many vertical units the item spans. Default is 1. |
| `ColumnSpan` | `int` | How many horizontal units the item spans. Default is 1. |
