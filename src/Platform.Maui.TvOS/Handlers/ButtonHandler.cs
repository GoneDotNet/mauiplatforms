using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class ButtonHandler : TvOSViewHandler<IButton, UIButton>
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

    protected override UIButton CreatePlatformView()
    {
        return new UIButton(UIButtonType.System);
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
        VirtualView?.Clicked();
    }

    public static void MapText(ButtonHandler handler, IButton button)
    {
        if (button is IText textButton)
            handler.PlatformView.SetTitle(textButton.Text, UIControlState.Normal);
    }

    public static void MapTextColor(ButtonHandler handler, IButton button)
    {
        if (button is ITextStyle textStyle && textStyle.TextColor != null)
            handler.PlatformView.SetTitleColor(textStyle.TextColor.ToPlatformColor(), UIControlState.Normal);
    }

    public static void MapBackground(ButtonHandler handler, IButton button)
    {
        if (button.Background is Microsoft.Maui.Graphics.SolidPaint solidPaint && solidPaint.Color != null)
            handler.PlatformView.BackgroundColor = solidPaint.Color.ToPlatformColor();
    }
}
