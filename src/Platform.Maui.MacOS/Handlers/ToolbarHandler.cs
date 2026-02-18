using System.Collections.Specialized;
using Foundation;
using Microsoft.Maui.Controls;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

/// <summary>
/// Manages an NSToolbar on the NSWindow, populated from Page.ToolbarItems.
/// Attach to a window via <see cref="MacOSToolbarManager.SetToolbarItems"/>.
/// </summary>
public class MacOSToolbarManager : NSObject, INSToolbarDelegate
{
    const string ToolbarId = "MauiToolbar";
    const string ItemIdPrefix = "MauiToolbarItem_";
    const string FlexibleSpaceId = "NSToolbarFlexibleSpaceItem";

    NSWindow? _window;
    NSToolbar? _toolbar;
    readonly List<ToolbarItem> _items = new();
    readonly List<string> _itemIdentifiers = new();
    Page? _currentPage;

    public void AttachToWindow(NSWindow window)
    {
        _window = window;
        _toolbar = new NSToolbar(ToolbarId)
        {
            Delegate = this,
            DisplayMode = NSToolbarDisplayMode.IconAndLabel,
            AllowsUserCustomization = false,
        };
        _window.Toolbar = _toolbar;
    }

    public void SetPage(Page? page)
    {
        if (_currentPage != null)
        {
            UnsubscribeCommands();
            if (_currentPage.ToolbarItems is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= OnToolbarItemsChanged;
        }

        _currentPage = page;

        if (_currentPage != null)
        {
            if (_currentPage.ToolbarItems is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += OnToolbarItemsChanged;
            RefreshToolbar(_currentPage.ToolbarItems);
        }
        else
        {
            RefreshToolbar(null);
        }
    }

    void OnToolbarItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_currentPage != null)
            RefreshToolbar(_currentPage.ToolbarItems);
    }

    void UnsubscribeCommands()
    {
        foreach (var item in _items)
            item.PropertyChanged -= OnToolbarItemPropertyChanged;
    }

    void RefreshToolbar(IList<ToolbarItem>? toolbarItems)
    {
        UnsubscribeCommands();
        _items.Clear();
        _itemIdentifiers.Clear();

        if (toolbarItems != null)
        {
            int index = 0;
            foreach (var item in toolbarItems)
            {
                if (item.Order == ToolbarItemOrder.Secondary)
                    continue; // Only primary items in the NSToolbar

                var id = $"{ItemIdPrefix}{index}";
                _items.Add(item);
                _itemIdentifiers.Add(id);
                item.PropertyChanged += OnToolbarItemPropertyChanged;
                index++;
            }
        }

        // Force NSToolbar to reload by removing and re-inserting items
        if (_toolbar != null)
        {
            while (_toolbar.Items.Length > 0)
                _toolbar.RemoveItem(0);

            for (int i = 0; i < _itemIdentifiers.Count; i++)
                _toolbar.InsertItem(_itemIdentifiers[i], i);
        }
    }

    void OnToolbarItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Re-validate toolbar items when properties change (e.g., IsEnabled via CanExecute)
        if (_toolbar != null)
            _toolbar.ValidateVisibleItems();
    }

    // INSToolbarDelegate
    [Export("toolbar:itemForItemIdentifier:willBeInsertedIntoToolbar:")]
    public NSToolbarItem ToolbarItemForIdentifier(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
    {
        if (itemIdentifier.StartsWith(ItemIdPrefix))
        {
            var indexStr = itemIdentifier.Substring(ItemIdPrefix.Length);
            if (int.TryParse(indexStr, out int index) && index >= 0 && index < _items.Count)
            {
                var mauiItem = _items[index];
                var nsItem = new NSToolbarItem(itemIdentifier)
                {
                    Label = mauiItem.Text ?? string.Empty,
                    PaletteLabel = mauiItem.Text ?? string.Empty,
                    ToolTip = mauiItem.Text ?? string.Empty,
                    Enabled = mauiItem.IsEnabled,
                    Target = this,
                    Action = new ObjCRuntime.Selector("toolbarItemClicked:"),
                    Tag = index,
                };

                // Use a standard NSButton as the toolbar item's view for a native look
                var button = new NSButton
                {
                    Title = mauiItem.Text ?? string.Empty,
                    BezelStyle = NSBezelStyle.TexturedRounded,
                    Tag = index,
                    Target = this,
                    Action = new ObjCRuntime.Selector("toolbarItemClicked:"),
                };
                nsItem.View = button;

                return nsItem;
            }
        }

        return new NSToolbarItem(itemIdentifier);
    }

    [Export("toolbarAllowedItemIdentifiers:")]
    public string[] ToolbarAllowedItemIdentifiers(NSToolbar toolbar)
    {
        var ids = new List<string>(_itemIdentifiers) { FlexibleSpaceId };
        return ids.ToArray();
    }

    [Export("toolbarDefaultItemIdentifiers:")]
    public string[] ToolbarDefaultItemIdentifiers(NSToolbar toolbar)
    {
        return _itemIdentifiers.ToArray();
    }

    [Export("toolbarItemClicked:")]
    void OnToolbarItemClicked(NSObject sender)
    {
        nint tag = -1;
        if (sender is NSToolbarItem item)
            tag = item.Tag;
        else if (sender is NSButton button)
            tag = button.Tag;

        if (tag >= 0 && tag < _items.Count)
        {
            var mauiItem = _items[(int)tag];
            if (mauiItem.IsEnabled)
                ((IMenuItemController)mauiItem).Activate();
        }
    }

    public void Detach()
    {
        SetPage(null);
        if (_window != null)
        {
            _window.Toolbar = null;
            _window = null;
        }
        _toolbar = null;
    }
}
