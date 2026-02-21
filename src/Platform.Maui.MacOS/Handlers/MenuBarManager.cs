using AppKit;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

/// <summary>
/// Manages the native macOS menu bar (NSApp.MainMenu) from MAUI Page.MenuBarItems.
/// macOS has a global menu bar, so this builds NSMenu/NSMenuItem hierarchy from
/// MenuBarItem, MenuFlyoutItem, and MenuFlyoutSeparator definitions.
/// </summary>
public static class MenuBarManager
{
    public static void UpdateMenuBar(IList<MenuBarItem>? menuBarItems)
    {
        var mainMenu = NSApplication.SharedApplication.MainMenu;
        if (mainMenu == null)
        {
            mainMenu = new NSMenu();
            NSApplication.SharedApplication.MainMenu = mainMenu;
        }

        // Keep the application menu (index 0) if it exists
        var appMenuItem = mainMenu.Count > 0 ? mainMenu.ItemAt(0) : null;
        mainMenu.RemoveAllItems();

        if (appMenuItem != null)
            mainMenu.AddItem(appMenuItem);
        else
            AddDefaultAppMenu(mainMenu);

        if (menuBarItems == null)
            return;

        foreach (var menuBarItem in menuBarItems)
        {
            var nsMenuItem = new NSMenuItem(menuBarItem.Text ?? string.Empty);
            var submenu = new NSMenu(menuBarItem.Text ?? string.Empty);

            foreach (var element in menuBarItem)
            {
                switch (element)
                {
                    case MenuFlyoutSeparator:
                        submenu.AddItem(NSMenuItem.SeparatorItem);
                        break;
                    case MenuFlyoutSubItem subItem:
                        var subMenuItem = CreateSubMenuItem(subItem);
                        submenu.AddItem(subMenuItem);
                        break;
                    case MenuFlyoutItem flyoutItem:
                        var item = CreateMenuItem(flyoutItem);
                        submenu.AddItem(item);
                        break;
                }
            }

            nsMenuItem.Submenu = submenu;
            mainMenu.AddItem(nsMenuItem);
        }
    }

    static NSMenuItem CreateMenuItem(MenuFlyoutItem flyoutItem)
    {
        var keyEquivalent = flyoutItem.KeyboardAccelerators?.FirstOrDefault();
        var nsItem = new NSMenuItem(
            flyoutItem.Text ?? string.Empty,
            new ObjCRuntime.Selector("menuItemClicked:"),
            keyEquivalent != null ? GetKeyEquivalent(keyEquivalent) : string.Empty);

        nsItem.Enabled = flyoutItem.IsEnabled;
        nsItem.RepresentedObject = new MenuItemCommandWrapper(flyoutItem);

        if (keyEquivalent != null)
            nsItem.KeyEquivalentModifierMask = GetModifierMask(keyEquivalent);

        return nsItem;
    }

    static NSMenuItem CreateSubMenuItem(MenuFlyoutSubItem subItem)
    {
        var nsItem = new NSMenuItem(subItem.Text ?? string.Empty);
        var submenu = new NSMenu(subItem.Text ?? string.Empty);

        foreach (var element in subItem)
        {
            switch (element)
            {
                case MenuFlyoutSeparator:
                    submenu.AddItem(NSMenuItem.SeparatorItem);
                    break;
                case MenuFlyoutSubItem nested:
                    submenu.AddItem(CreateSubMenuItem(nested));
                    break;
                case MenuFlyoutItem flyoutItem:
                    submenu.AddItem(CreateMenuItem(flyoutItem));
                    break;
            }
        }

        nsItem.Submenu = submenu;
        return nsItem;
    }

    static void AddDefaultAppMenu(NSMenu mainMenu)
    {
        var appMenu = new NSMenuItem();
        var appSubmenu = new NSMenu();

        var quitItem = new NSMenuItem(
            $"Quit",
            new ObjCRuntime.Selector("terminate:"),
            "q");
        appSubmenu.AddItem(quitItem);

        appMenu.Submenu = appSubmenu;
        mainMenu.AddItem(appMenu);
    }

    static string GetKeyEquivalent(KeyboardAccelerator accelerator)
    {
        return accelerator.Key?.ToLower() ?? string.Empty;
    }

    static NSEventModifierMask GetModifierMask(KeyboardAccelerator accelerator)
    {
        var mask = NSEventModifierMask.CommandKeyMask;
        if (accelerator.Modifiers.HasFlag(KeyboardAcceleratorModifiers.Shift))
            mask |= NSEventModifierMask.ShiftKeyMask;
        if (accelerator.Modifiers.HasFlag(KeyboardAcceleratorModifiers.Alt))
            mask |= NSEventModifierMask.AlternateKeyMask;
        if (accelerator.Modifiers.HasFlag(KeyboardAcceleratorModifiers.Ctrl))
            mask |= NSEventModifierMask.ControlKeyMask;
        return mask;
    }
}

/// <summary>
/// Wraps a MenuFlyoutItem's command for invocation from NSMenuItem action.
/// </summary>
internal class MenuItemCommandWrapper : Foundation.NSObject
{
    readonly MenuFlyoutItem _flyoutItem;

    public MenuItemCommandWrapper(MenuFlyoutItem flyoutItem)
    {
        _flyoutItem = flyoutItem;
    }

    [Foundation.Export("menuItemClicked:")]
    public void MenuItemClicked(Foundation.NSObject sender)
    {
        _flyoutItem.Command?.Execute(_flyoutItem.CommandParameter);
        (_flyoutItem as IMenuFlyoutItem)?.Clicked();
    }
}
