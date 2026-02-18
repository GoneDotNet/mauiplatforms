using Foundation;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class EntryHandler : TvOSViewHandler<IEntry, UITextField>
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
            [nameof(IEntry.ReturnType)] = MapReturnType,
            [nameof(ITextInput.MaxLength)] = MapMaxLength,
        };

    bool _updating;

    public EntryHandler() : base(Mapper)
    {
    }

    protected override UITextField CreatePlatformView()
    {
        return new UITextField
        {
            BorderStyle = UITextBorderStyle.RoundedRect,
        };
    }

    protected override void ConnectHandler(UITextField platformView)
    {
        base.ConnectHandler(platformView);
        platformView.EditingChanged += OnEditingChanged;
        platformView.ShouldReturn = OnShouldReturn;
    }

    protected override void DisconnectHandler(UITextField platformView)
    {
        platformView.EditingChanged -= OnEditingChanged;
        platformView.ShouldReturn = null;
        base.DisconnectHandler(platformView);
    }

    void OnEditingChanged(object? sender, EventArgs e)
    {
        if (_updating || VirtualView == null)
            return;

        _updating = true;
        try
        {
            if (VirtualView is ITextInput textInput)
                textInput.Text = PlatformView.Text ?? string.Empty;
        }
        finally
        {
            _updating = false;
        }
    }

    bool OnShouldReturn(UITextField textField)
    {
        textField.ResignFirstResponder();
        VirtualView?.Completed();
        return true;
    }

    public static void MapText(EntryHandler handler, IEntry entry)
    {
        if (handler._updating)
            return;

        if (entry is ITextInput textInput)
            handler.PlatformView.Text = textInput.Text;
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
            var fontSize = textStyle.Font.Size > 0 ? (nfloat)textStyle.Font.Size : (nfloat)17.0;
            handler.PlatformView.Font = UIFont.SystemFontOfSize(fontSize);
        }
    }

    public static void MapPlaceholder(EntryHandler handler, IEntry entry)
    {
        if (entry is IPlaceholder placeholder)
            handler.PlatformView.Placeholder = placeholder.Placeholder;
    }

    public static void MapPlaceholderColor(EntryHandler handler, IEntry entry)
    {
        if (entry is IPlaceholder placeholder && placeholder.PlaceholderColor != null)
        {
            handler.PlatformView.AttributedPlaceholder = new NSAttributedString(
                placeholder.Placeholder ?? string.Empty,
                new UIStringAttributes
                {
                    ForegroundColor = placeholder.PlaceholderColor.ToPlatformColor(),
                });
        }
    }

    public static void MapIsPassword(EntryHandler handler, IEntry entry)
    {
        handler.PlatformView.SecureTextEntry = entry.IsPassword;
    }

    public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
    {
        if (entry is ITextInput textInput)
            handler.PlatformView.Enabled = !textInput.IsReadOnly;
    }

    public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
    {
        if (entry is ITextAlignment textAlignment)
        {
            handler.PlatformView.TextAlignment = textAlignment.HorizontalTextAlignment switch
            {
                TextAlignment.Center => UITextAlignment.Center,
                TextAlignment.End => UITextAlignment.Right,
                _ => UITextAlignment.Left,
            };
        }
    }

    public static void MapReturnType(EntryHandler handler, IEntry entry)
    {
        handler.PlatformView.ReturnKeyType = entry.ReturnType switch
        {
            ReturnType.Go => UIReturnKeyType.Go,
            ReturnType.Next => UIReturnKeyType.Next,
            ReturnType.Search => UIReturnKeyType.Search,
            ReturnType.Send => UIReturnKeyType.Send,
            ReturnType.Done => UIReturnKeyType.Done,
            _ => UIReturnKeyType.Default,
        };
    }

    public static void MapMaxLength(EntryHandler handler, IEntry entry)
    {
        // MaxLength enforcement is handled during text change events
        if (entry is ITextInput textInput && textInput.MaxLength >= 0)
        {
            var currentText = handler.PlatformView.Text ?? string.Empty;
            if (currentText.Length > textInput.MaxLength)
                handler.PlatformView.Text = currentText[..textInput.MaxLength];
        }
    }
}
