using Foundation;
using Microsoft.Maui.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

public partial class EntryHandler : MacOSViewHandler<IEntry, NSTextField>
{
    public static readonly IPropertyMapper<IEntry, EntryHandler> Mapper =
        new PropertyMapper<IEntry, EntryHandler>(ViewMapper)
        {
            [nameof(ITextInput.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
            [nameof(IPlaceholder.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(IEntry.IsPassword)] = MapIsPassword,
            [nameof(IEntry.ReturnType)] = MapReturnType,
            [nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
            [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(ITextInput.MaxLength)] = MapMaxLength,
        };

    bool _updating;

    public EntryHandler() : base(Mapper)
    {
    }

    protected override NSTextField CreatePlatformView()
    {
        return new NSTextField
        {
            Bordered = true,
            Bezeled = true,
            BezelStyle = NSTextFieldBezelStyle.Rounded,
        };
    }

    protected override void ConnectHandler(NSTextField platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Changed += OnTextChanged;
        platformView.EditingEnded += OnEditingEnded;
    }

    protected override void DisconnectHandler(NSTextField platformView)
    {
        platformView.Changed -= OnTextChanged;
        platformView.EditingEnded -= OnEditingEnded;
        base.DisconnectHandler(platformView);
    }

    void OnTextChanged(object? sender, EventArgs e)
    {
        if (_updating || VirtualView == null)
            return;

        _updating = true;
        try
        {
            if (VirtualView is ITextInput textInput)
                textInput.Text = PlatformView.StringValue ?? string.Empty;
        }
        finally
        {
            _updating = false;
        }
    }

    void OnEditingEnded(object? sender, EventArgs e)
    {
        VirtualView?.Completed();
    }

    public static void MapText(EntryHandler handler, IEntry entry)
    {
        if (handler._updating)
            return;

        if (entry is ITextInput textInput)
            handler.PlatformView.StringValue = textInput.Text ?? string.Empty;
    }

    public static void MapTextColor(EntryHandler handler, IEntry entry)
    {
        if (entry is ITextStyle textStyle && textStyle.TextColor != null)
            handler.PlatformView.TextColor = textStyle.TextColor.ToPlatformColor();
    }

    public static void MapFont(EntryHandler handler, IEntry entry)
    {
        if (entry is ITextStyle textStyle)
            handler.PlatformView.Font = textStyle.Font.ToNSFont();
    }

    public static void MapPlaceholder(EntryHandler handler, IEntry entry)
    {
        if (entry is IPlaceholder placeholder)
            handler.PlatformView.PlaceholderString = placeholder.Placeholder ?? string.Empty;
    }

    public static void MapPlaceholderColor(EntryHandler handler, IEntry entry)
    {
        if (entry is IPlaceholder placeholder && placeholder.PlaceholderColor != null)
        {
            var attributes = new NSDictionary(
                NSStringAttributeKey.ForegroundColor,
                placeholder.PlaceholderColor.ToPlatformColor());

            handler.PlatformView.PlaceholderAttributedString = new NSAttributedString(
                placeholder.Placeholder ?? string.Empty,
                attributes);
        }
    }

    public static void MapIsPassword(EntryHandler handler, IEntry entry)
    {
        // NSSecureTextField is a separate class on macOS — we can't toggle it on an existing NSTextField.
        // Instead, we use the UsesSingleLineMode + cell replacement approach isn't available either.
        // The practical approach: rebuild the platform view when IsPassword changes.
        // For now, apply secure text entry via the field editor when it becomes the first responder.
        // A full implementation would require swapping between NSTextField and NSSecureTextField.
        // TODO: Consider implementing platform view swap for IsPassword toggle
    }

    public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
    {
        if (entry is ITextInput textInput)
            handler.PlatformView.Editable = !textInput.IsReadOnly;
    }

    public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
    {
        if (entry is ITextAlignment textAlignment)
        {
            handler.PlatformView.Alignment = textAlignment.HorizontalTextAlignment switch
            {
                TextAlignment.Center => NSTextAlignment.Center,
                TextAlignment.End => NSTextAlignment.Right,
                _ => NSTextAlignment.Left,
            };
        }
    }

    public static void MapMaxLength(EntryHandler handler, IEntry entry)
    {
        if (entry is ITextInput textInput && textInput.MaxLength >= 0)
        {
            var currentText = handler.PlatformView.StringValue ?? string.Empty;
            if (currentText.Length > textInput.MaxLength)
                handler.PlatformView.StringValue = currentText[..textInput.MaxLength];
        }
    }

    public static void MapCharacterSpacing(EntryHandler handler, IEntry entry)
    {
        if (entry is ITextStyle textStyle && textStyle.CharacterSpacing != 0)
        {
            var text = handler.PlatformView.StringValue ?? string.Empty;
            var attrStr = new NSMutableAttributedString(text);
            attrStr.AddAttribute(NSStringAttributeKey.KerningAdjustment,
                NSNumber.FromDouble(textStyle.CharacterSpacing), new NSRange(0, text.Length));
            handler.PlatformView.AttributedStringValue = attrStr;
        }
    }

    public static void MapReturnType(EntryHandler handler, IEntry entry)
    {
        // macOS NSTextField doesn't have a ReturnType concept like iOS keyboard return key.
        // The return key always submits the field on macOS.
    }
}
