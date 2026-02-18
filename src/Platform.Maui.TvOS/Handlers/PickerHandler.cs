using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class PickerHandler : TvOSViewHandler<IPicker, UIButton>
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

    protected override UIButton CreatePlatformView()
    {
        var button = new UIButton(UIButtonType.System);
        button.SetTitle("Select...", UIControlState.Normal);
        return button;
    }

    protected override void ConnectHandler(UIButton platformView)
    {
        base.ConnectHandler(platformView);
        platformView.PrimaryActionTriggered += OnPrimaryActionTriggered;
    }

    protected override void DisconnectHandler(UIButton platformView)
    {
        platformView.PrimaryActionTriggered -= OnPrimaryActionTriggered;
        base.DisconnectHandler(platformView);
    }

    void OnPrimaryActionTriggered(object? sender, EventArgs e)
    {
        if (VirtualView == null)
            return;

        var alert = UIAlertController.Create(
            VirtualView.Title ?? "Select",
            null,
            UIAlertControllerStyle.ActionSheet);

        for (int i = 0; i < VirtualView.Items.Count; i++)
        {
            var index = i;
            var action = UIAlertAction.Create(
                VirtualView.Items[i],
                UIAlertActionStyle.Default,
                _ =>
                {
                    VirtualView.SelectedIndex = index;
                    UpdateButtonTitle();
                });
            alert.AddAction(action);
        }

        alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

        var viewController = GetViewController();
        viewController?.PresentViewController(alert, true, null);
    }

    UIViewController? GetViewController()
    {
        UIResponder? responder = PlatformView;
        while (responder != null)
        {
            if (responder is UIViewController vc)
                return vc;
            responder = responder.NextResponder;
        }
        return null;
    }

    void UpdateButtonTitle()
    {
        if (VirtualView == null)
            return;

        if (VirtualView.SelectedIndex >= 0 && VirtualView.SelectedIndex < VirtualView.Items.Count)
            PlatformView.SetTitle(VirtualView.Items[VirtualView.SelectedIndex], UIControlState.Normal);
        else
            PlatformView.SetTitle(VirtualView.Title ?? "Select...", UIControlState.Normal);
    }

    public static void MapTitle(PickerHandler handler, IPicker picker)
    {
        handler.UpdateButtonTitle();
    }

    public static void MapTitleColor(PickerHandler handler, IPicker picker)
    {
        // Title color applies when no item is selected (showing placeholder title)
        if (picker.SelectedIndex < 0 && picker.TitleColor != null)
            handler.PlatformView.SetTitleColor(picker.TitleColor.ToPlatformColor(), UIControlState.Normal);
    }

    public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
    {
        handler.UpdateButtonTitle();
    }

    public static void MapItems(PickerHandler handler, IPicker picker)
    {
        // If current selection is out of range, reset
        if (picker.SelectedIndex >= picker.Items.Count)
            picker.SelectedIndex = -1;

        handler.UpdateButtonTitle();
    }

    public static void MapTextColor(PickerHandler handler, IPicker picker)
    {
        if (picker is ITextStyle textStyle && textStyle.TextColor != null)
            handler.PlatformView.SetTitleColor(textStyle.TextColor.ToPlatformColor(), UIControlState.Normal);
    }

    public static void MapBackground(PickerHandler handler, IPicker picker)
    {
        if (picker.Background is Microsoft.Maui.Graphics.SolidPaint solidPaint && solidPaint.Color != null)
            handler.PlatformView.BackgroundColor = solidPaint.Color.ToPlatformColor();
    }
}
