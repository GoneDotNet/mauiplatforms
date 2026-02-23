# Window Configuration

Customize the macOS window titlebar appearance, toolbar style, and content layout.

## Titlebar Style

Control how the toolbar integrates with the titlebar:

```csharp
MacOSWindow.SetTitlebarStyle(window, MacOSTitlebarStyle.UnifiedCompact);
```

| Style | Description |
|-------|-------------|
| `Automatic` | System default |
| `Expanded` | Full-height toolbar below the titlebar |
| `Preference` | Centered icons with labels (like System Preferences) |
| `Unified` | Toolbar items inline with the titlebar |
| `UnifiedCompact` | Compact unified style with smaller toolbar |

## Transparent Titlebar

Make the titlebar transparent so content can extend behind it:

```csharp
MacOSWindow.SetTitlebarTransparent(window, true);
```

This is commonly used with `FullSizeContentView` for edge-to-edge content layouts.

## Title Visibility

Show or hide the window title text:

```csharp
MacOSWindow.SetTitleVisibility(window, MacOSTitleVisibility.Hidden);
```

| Value | Description |
|-------|-------------|
| `Visible` | Show the title text (default) |
| `Hidden` | Hide the title text |

## Example: Modern App Appearance

Combine these properties for a modern, unified look:

```csharp
// In your App class or Window handler
MacOSWindow.SetTitlebarStyle(window, MacOSTitlebarStyle.UnifiedCompact);
MacOSWindow.SetTitlebarTransparent(window, true);
MacOSWindow.SetTitleVisibility(window, MacOSTitleVisibility.Hidden);
```

## API Reference

### MacOSWindow (Attached Properties)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TitlebarStyle` | `MacOSTitlebarStyle` | `Automatic` | Toolbar/titlebar integration style |
| `TitlebarTransparent` | `bool` | `false` | Transparent titlebar |
| `TitleVisibility` | `MacOSTitleVisibility` | `Visible` | Show/hide title text |

### MacOSTitlebarStyle Enum

| Value | NSToolbarStyle | Description |
|-------|----------------|-------------|
| `Automatic` | `.Automatic` | System decides |
| `Expanded` | `.Expanded` | Separate toolbar row |
| `Preference` | `.Preference` | Centered toolbar (Settings-style) |
| `Unified` | `.Unified` | Inline with titlebar |
| `UnifiedCompact` | `.UnifiedCompact` | Compact inline |

### MacOSTitleVisibility Enum

| Value | NSTitleVisibility | Description |
|-------|-------------------|-------------|
| `Visible` | `.Visible` | Title shown |
| `Hidden` | `.Hidden` | Title hidden |
