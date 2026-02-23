# macOS (AppKit) Platform APIs

This .NET MAUI backend provides native macOS experiences using AppKit. The following platform-specific APIs let you build apps that look and feel like first-class Mac citizens.

## Guides

| Topic | Description |
|-------|-------------|
| [Sidebar Navigation](sidebar.md) | Native `NSSplitViewController` sidebar for Shell and FlyoutPage |
| [Toolbar](toolbar.md) | NSToolbar with native item types, placement, and layout |
| [Toolbar Item Types](toolbar-items.md) | Search, menu, segmented control, share, and popup toolbar items |
| [Window Configuration](window.md) | Titlebar style, transparency, and toolbar style |

## Quick Start

Register the macOS backend in your app:

```csharp
// MauiProgram.cs
builder.UseMauiApp<App>()
       .UseMacOS();
```

### Native Sidebar with Shell

```csharp
var shell = new Shell();
MacOSShell.SetUseNativeSidebar(shell, true);
```

### Toolbar Items

```csharp
// Add a toolbar item to the sidebar area
var item = new ToolbarItem { Text = "Refresh", IconImageSource = "arrow.clockwise" };
MacOSToolbarItem.SetPlacement(item, MacOSToolbarItemPlacement.SidebarLeading);
page.ToolbarItems.Add(item);
```

### Window Titlebar

```csharp
MacOSWindow.SetTitlebarStyle(window, MacOSTitlebarStyle.UnifiedCompact);
MacOSWindow.SetTitlebarTransparent(window, true);
```

## Platform-Specific Namespace

All macOS APIs are in the `Microsoft.Maui.Platform.MacOS` namespace:

```csharp
using Microsoft.Maui.Platform.MacOS;
```
