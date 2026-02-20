using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;
using AppKit;
using Foundation;

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
            [nameof(Label.LineBreakMode)] = MapLineBreakMode,
            [nameof(Label.MaxLines)] = MapMaxLines,
            [nameof(ILabel.TextDecorations)] = MapTextDecorations,
            [nameof(ILabel.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ILabel.Padding)] = MapPadding,
            [nameof(Label.FormattedText)] = MapFormattedText,
        };

    public LabelHandler() : base(Mapper)
    {
    }

    protected override NSTextField CreatePlatformView()
    {
        var label = NSTextField.CreateLabel(string.Empty);
        label.Editable = false;
        label.Selectable = false;
        label.Bordered = false;
        label.DrawsBackground = false;
        label.TextColor = NSColor.White;
        label.MaximumNumberOfLines = 0;
        return label;
    }

    public static void MapText(LabelHandler handler, ILabel label)
    {
        if (label is Label mauiLabel && mauiLabel.FormattedText != null)
            return; // FormattedText takes precedence
        handler.PlatformView.StringValue = label.Text ?? string.Empty;
    }

    public static void MapTextColor(LabelHandler handler, ILabel label)
    {
        if (label.TextColor != null)
            handler.PlatformView.TextColor = label.TextColor.ToPlatformColor();
    }

    public static void MapFont(LabelHandler handler, ILabel label)
    {
        handler.PlatformView.Font = label.Font.ToNSFont();
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

    public static void MapLineBreakMode(LabelHandler handler, ILabel label)
    {
        if (label is not Label mauiLabel)
            return;

        handler.PlatformView.LineBreakMode = mauiLabel.LineBreakMode switch
        {
            LineBreakMode.NoWrap => NSLineBreakMode.Clipping,
            LineBreakMode.CharacterWrap => NSLineBreakMode.CharWrapping,
            LineBreakMode.HeadTruncation => NSLineBreakMode.TruncatingHead,
            LineBreakMode.TailTruncation => NSLineBreakMode.TruncatingTail,
            LineBreakMode.MiddleTruncation => NSLineBreakMode.TruncatingMiddle,
            _ => NSLineBreakMode.ByWordWrapping,
        };
    }

    public static void MapMaxLines(LabelHandler handler, ILabel label)
    {
        if (label is Label mauiLabel)
            handler.PlatformView.MaximumNumberOfLines = mauiLabel.MaxLines;
    }

    public static void MapTextDecorations(LabelHandler handler, ILabel label)
    {
        ApplyTextDecorations(handler.PlatformView, label.TextDecorations);
    }

    public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
    {
        if (label.CharacterSpacing != 0)
        {
            var text = handler.PlatformView.StringValue ?? string.Empty;
            var attrStr = new NSMutableAttributedString(text);
            attrStr.AddAttribute(NSStringAttributeKey.KerningAdjustment,
                NSNumber.FromDouble(label.CharacterSpacing), new NSRange(0, text.Length));
            handler.PlatformView.AttributedStringValue = attrStr;
        }
    }

    public static void MapPadding(LabelHandler handler, ILabel label)
    {
        // NSTextField doesn't natively support padding; handled by MAUI layout via Margin
    }

    public static void MapFormattedText(LabelHandler handler, ILabel label)
    {
        if (label is not Label mauiLabel)
            return;

        var formattedText = mauiLabel.FormattedText;
        if (formattedText == null || formattedText.Spans.Count == 0)
        {
            handler.PlatformView.StringValue = label.Text ?? string.Empty;
            return;
        }

        var attributed = new NSMutableAttributedString();

        foreach (var span in formattedText.Spans)
        {
            var text = span.Text ?? string.Empty;
            var attrs = new NSMutableDictionary();

            // Font
            var font = SpanToNSFont(span);
            attrs[NSStringAttributeKey.Font] = font;

            // TextColor
            if (span.TextColor != null)
                attrs[NSStringAttributeKey.ForegroundColor] = span.TextColor.ToPlatformColor();
            else if (label.TextColor != null)
                attrs[NSStringAttributeKey.ForegroundColor] = label.TextColor.ToPlatformColor();

            // BackgroundColor
            if (span.BackgroundColor != null)
                attrs[NSStringAttributeKey.BackgroundColor] = span.BackgroundColor.ToPlatformColor();

            // CharacterSpacing
            if (span.CharacterSpacing != 0)
                attrs[NSStringAttributeKey.KerningAdjustment] = NSNumber.FromDouble(span.CharacterSpacing);

            // TextDecorations
            if (span.TextDecorations.HasFlag(TextDecorations.Underline))
                attrs[NSStringAttributeKey.UnderlineStyle] = NSNumber.FromInt32((int)NSUnderlineStyle.Single);
            if (span.TextDecorations.HasFlag(TextDecorations.Strikethrough))
                attrs[NSStringAttributeKey.StrikethroughStyle] = NSNumber.FromInt32((int)NSUnderlineStyle.Single);

            var spanStr = new NSAttributedString(text, attrs);
            attributed.Append(spanStr);
        }

        handler.PlatformView.AttributedStringValue = attributed;
    }

    static NSFont SpanToNSFont(Span span)
    {
        var size = span.FontSize > 0 ? (nfloat)span.FontSize : (nfloat)13.0;
        NSFont? nsFont = null;

        if (!string.IsNullOrEmpty(span.FontFamily))
            nsFont = NSFont.FromFontName(span.FontFamily, size);

        nsFont ??= NSFont.SystemFontOfSize(size);

        var manager = NSFontManager.SharedFontManager;
        if (span.FontAttributes.HasFlag(FontAttributes.Bold))
            nsFont = manager.ConvertFont(nsFont, NSFontTraitMask.Bold) ?? nsFont;
        if (span.FontAttributes.HasFlag(FontAttributes.Italic))
            nsFont = manager.ConvertFont(nsFont, NSFontTraitMask.Italic) ?? nsFont;

        return nsFont;
    }

    static void ApplyTextDecorations(NSTextField textField, TextDecorations decorations)
    {
        var text = textField.StringValue ?? string.Empty;
        if (string.IsNullOrEmpty(text))
            return;

        var attrStr = new NSMutableAttributedString(text);
        var range = new NSRange(0, text.Length);

        if (decorations.HasFlag(TextDecorations.Underline))
            attrStr.AddAttribute(NSStringAttributeKey.UnderlineStyle,
                NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);

        if (decorations.HasFlag(TextDecorations.Strikethrough))
            attrStr.AddAttribute(NSStringAttributeKey.StrikethroughStyle,
                NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);

        textField.AttributedStringValue = attrStr;
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

internal static class FontExtensions
{
    public static NSFont ToNSFont(this Font font)
    {
        var size = font.Size > 0 ? (nfloat)font.Size : (nfloat)13.0;

        NSFont? nsFont = null;

        if (!string.IsNullOrEmpty(font.Family))
            nsFont = NSFont.FromFontName(font.Family, size);

        nsFont ??= NSFont.SystemFontOfSize(size);

        var manager = NSFontManager.SharedFontManager;
        if (font.Weight >= FontWeight.Bold)
            nsFont = manager.ConvertFont(nsFont, NSFontTraitMask.Bold) ?? nsFont;

        return nsFont;
    }
}
