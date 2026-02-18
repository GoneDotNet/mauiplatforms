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
            [nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
            [nameof(IPlaceholder.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(IEntry.IsPassword)] = MapIsPassword,
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
        {
            var fontSize = textStyle.Font.Size > 0 ? (nfloat)textStyle.Font.Size : (nfloat)13.0;
            handler.PlatformView.Font = NSFont.SystemFontOfSize(fontSize);
        }
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
        // NSSecureTextField is a separate class on macOS.
        // For simplicity, we don't swap the control type dynamically here.
        // A production implementation would need to swap between NSTextField and NSSecureTextField.
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
}
