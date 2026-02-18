using Microsoft.Maui.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

public partial class PickerHandler : MacOSViewHandler<IPicker, NSPopUpButton>
{
    public static readonly IPropertyMapper<IPicker, PickerHandler> Mapper =
        new PropertyMapper<IPicker, PickerHandler>(ViewMapper)
        {
            [nameof(IPicker.Title)] = MapTitle,
            [nameof(IPicker.TitleColor)] = MapTitleColor,
            [nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
            [nameof(IPicker.Items)] = MapItems,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(IView.Background)] = MapBackground,
        };

    public PickerHandler() : base(Mapper)
    {
    }

    protected override NSPopUpButton CreatePlatformView()
    {
        var popup = new NSPopUpButton(new CoreGraphics.CGRect(0, 0, 200, 25), false);
        return popup;
    }

    protected override void ConnectHandler(NSPopUpButton platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Activated += OnActivated;
    }

    protected override void DisconnectHandler(NSPopUpButton platformView)
    {
        platformView.Activated -= OnActivated;
        base.DisconnectHandler(platformView);
    }

    void OnActivated(object? sender, EventArgs e)
    {
        if (VirtualView == null)
            return;

        var selectedIndex = (int)PlatformView.IndexOfSelectedItem;
        // Account for the placeholder item at index 0
        if (VirtualView.Title != null)
            selectedIndex -= 1;

        if (selectedIndex >= 0 && selectedIndex < VirtualView.Items.Count)
            VirtualView.SelectedIndex = selectedIndex;
    }

    void RebuildItems()
    {
        if (VirtualView == null)
            return;

        PlatformView.RemoveAllItems();

        // Add placeholder title if present
        if (VirtualView.Title != null)
            PlatformView.AddItem(VirtualView.Title);

        foreach (var item in VirtualView.Items)
            PlatformView.AddItem(item);

        UpdateSelection();
    }

    void UpdateSelection()
    {
        if (VirtualView == null)
            return;

        var offset = VirtualView.Title != null ? 1 : 0;

        if (VirtualView.SelectedIndex >= 0 && VirtualView.SelectedIndex < VirtualView.Items.Count)
            PlatformView.SelectItem(VirtualView.SelectedIndex + offset);
        else if (offset > 0)
            PlatformView.SelectItem(0); // Select placeholder
    }

    public static void MapTitle(PickerHandler handler, IPicker picker)
    {
        handler.RebuildItems();
    }

    public static void MapTitleColor(PickerHandler handler, IPicker picker)
    {
        // Title color is managed through the NSPopUpButton's appearance
    }

    public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
    {
        handler.UpdateSelection();
    }

    public static void MapItems(PickerHandler handler, IPicker picker)
    {
        handler.RebuildItems();
    }

    public static void MapTextColor(PickerHandler handler, IPicker picker)
    {
        // NSPopUpButton uses system text color by default
    }

    public static void MapBackground(PickerHandler handler, IPicker picker)
    {
        if (picker.Background is Graphics.SolidPaint solidPaint && solidPaint.Color != null)
        {
            handler.PlatformView.WantsLayer = true;
            handler.PlatformView.Layer!.BackgroundColor = solidPaint.Color.ToPlatformColor().CGColor;
        }
    }
}
