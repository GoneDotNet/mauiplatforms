using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class LabelHandler : TvOSViewHandler<ILabel, UILabel>
{
    public static readonly IPropertyMapper<ILabel, LabelHandler> Mapper =
        new PropertyMapper<ILabel, LabelHandler>(ViewMapper)
        {
            [nameof(ILabel.Text)] = MapText,
            [nameof(ILabel.TextColor)] = MapTextColor,
            [nameof(ILabel.Font)] = MapFont,
            [nameof(ILabel.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        };

    public LabelHandler() : base(Mapper)
    {
    }

    protected override UILabel CreatePlatformView()
    {
        return new UILabel
        {
            Lines = 0,
            TextColor = UIColor.White,
        };
    }

    public static void MapText(LabelHandler handler, ILabel label)
    {
        handler.PlatformView.Text = label.Text;
    }

    public static void MapTextColor(LabelHandler handler, ILabel label)
    {
        if (label.TextColor != null)
            handler.PlatformView.TextColor = label.TextColor.ToPlatformColor();
    }

    public static void MapFont(LabelHandler handler, ILabel label)
    {
        var fontSize = label.Font.Size > 0 ? (nfloat)label.Font.Size : (nfloat)17.0;
        handler.PlatformView.Font = UIFont.SystemFontOfSize(fontSize);
    }

    public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
    {
        handler.PlatformView.TextAlignment = label.HorizontalTextAlignment switch
        {
            TextAlignment.Center => UITextAlignment.Center,
            TextAlignment.End => UITextAlignment.Right,
            _ => UITextAlignment.Left,
        };
    }
}

internal static class ColorExtensions
{
    public static UIColor ToPlatformColor(this Graphics.Color color)
    {
        if (color == null)
            return UIColor.White;

        return new UIColor(
            (nfloat)color.Red,
            (nfloat)color.Green,
            (nfloat)color.Blue,
            (nfloat)color.Alpha);
    }
}
