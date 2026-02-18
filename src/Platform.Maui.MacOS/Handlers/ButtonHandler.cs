using Microsoft.Maui.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

public partial class ButtonHandler : MacOSViewHandler<IButton, NSButton>
{
    public static readonly IPropertyMapper<IButton, ButtonHandler> Mapper =
        new PropertyMapper<IButton, ButtonHandler>(ViewMapper)
        {
            [nameof(IText.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(IView.Background)] = MapBackground,
        };

    public ButtonHandler() : base(Mapper)
    {
    }

    protected override NSButton CreatePlatformView()
    {
        var button = new NSButton
        {
            BezelStyle = NSBezelStyle.Rounded,
            Title = string.Empty,
        };
        return button;
    }

    protected override void ConnectHandler(NSButton platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Activated += OnActivated;
    }

    protected override void DisconnectHandler(NSButton platformView)
    {
        platformView.Activated -= OnActivated;
        base.DisconnectHandler(platformView);
    }

    void OnActivated(object? sender, EventArgs e)
    {
        VirtualView?.Clicked();
    }

    public static void MapText(ButtonHandler handler, IButton button)
    {
        if (button is IText textButton)
            handler.PlatformView.Title = textButton.Text ?? string.Empty;
    }

    public static void MapTextColor(ButtonHandler handler, IButton button)
    {
        if (button is ITextStyle textStyle && textStyle.TextColor != null)
            handler.PlatformView.ContentTintColor = textStyle.TextColor.ToPlatformColor();
    }

    public static void MapBackground(ButtonHandler handler, IButton button)
    {
        if (button.Background is Graphics.SolidPaint solidPaint && solidPaint.Color != null)
        {
            handler.PlatformView.WantsLayer = true;
            handler.PlatformView.Layer!.BackgroundColor = solidPaint.Color.ToPlatformColor().CGColor;
        }
    }
}
