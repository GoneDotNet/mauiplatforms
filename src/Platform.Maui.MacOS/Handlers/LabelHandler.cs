using Microsoft.Maui.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

public partial class LabelHandler : MacOSViewHandler<ILabel, NSTextField>
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

    protected override NSTextField CreatePlatformView()
    {
        // NSTextField as a non-editable label
        var label = NSTextField.CreateLabel(string.Empty);
        label.Editable = false;
        label.Selectable = false;
        label.Bordered = false;
        label.DrawsBackground = false;
        label.TextColor = NSColor.White;
        label.MaximumNumberOfLines = 0; // unlimited lines (like UILabel.Lines = 0)
        return label;
    }

    public static void MapText(LabelHandler handler, ILabel label)
    {
        handler.PlatformView.StringValue = label.Text ?? string.Empty;
    }

    public static void MapTextColor(LabelHandler handler, ILabel label)
    {
        if (label.TextColor != null)
            handler.PlatformView.TextColor = label.TextColor.ToPlatformColor();
    }

    public static void MapFont(LabelHandler handler, ILabel label)
    {
        var fontSize = label.Font.Size > 0 ? (nfloat)label.Font.Size : (nfloat)13.0;
        handler.PlatformView.Font = NSFont.SystemFontOfSize(fontSize);
    }

    public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
    {
        handler.PlatformView.Alignment = label.HorizontalTextAlignment switch
        {
            TextAlignment.Center => NSTextAlignment.Center,
            TextAlignment.End => NSTextAlignment.Right,
            _ => NSTextAlignment.Left,
        };
    }
}

internal static class ColorExtensions
{
    public static NSColor ToPlatformColor(this Graphics.Color color)
    {
        if (color == null)
            return NSColor.White;

        return NSColor.FromRgba(
            (nfloat)color.Red,
            (nfloat)color.Green,
            (nfloat)color.Blue,
            (nfloat)color.Alpha);
    }
}
