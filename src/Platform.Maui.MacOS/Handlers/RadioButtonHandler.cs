using Microsoft.Maui.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

public partial class RadioButtonHandler : MacOSViewHandler<IRadioButton, NSButton>
{
	public static readonly IPropertyMapper<IRadioButton, RadioButtonHandler> Mapper =
		new PropertyMapper<IRadioButton, RadioButtonHandler>(ViewMapper)
		{
			[nameof(IRadioButton.IsChecked)] = MapIsChecked,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(IText.Text)] = MapContent,
			[nameof(IContentView.Content)] = MapContent,
		};

	bool _updating;

	public RadioButtonHandler() : base(Mapper)
	{
	}

	protected override NSButton CreatePlatformView()
	{
		var button = new NSButton
		{
			Title = string.Empty,
		};
		button.SetButtonType(NSButtonType.Radio);
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
		if (_updating || VirtualView == null)
			return;

		_updating = true;
		try
		{
			VirtualView.IsChecked = PlatformView.State == NSCellStateValue.On;
		}
		finally
		{
			_updating = false;
		}
	}

	public static void MapIsChecked(RadioButtonHandler handler, IRadioButton view)
	{
		if (handler._updating)
			return;

		handler.PlatformView.State = view.IsChecked ? NSCellStateValue.On : NSCellStateValue.Off;
	}

	public static void MapTextColor(RadioButtonHandler handler, IRadioButton view)
	{
		if (view is ITextStyle textStyle && textStyle.TextColor != null)
			handler.PlatformView.ContentTintColor = textStyle.TextColor.ToPlatformColor();
	}

	public static void MapContent(RadioButtonHandler handler, IRadioButton view)
	{
		handler.PlatformView.Title = ExtractContentText(view) ?? string.Empty;
	}

	static string? ExtractContentText(IRadioButton view)
	{
		var content = view.Content;
		if (content == null)
			return null;

		if (content is string s)
			return s;

		// For View content, try to extract meaningful text instead of showing type name
		if (content is IText textView)
			return textView.Text;

		if (content is ILabel label)
			return label.Text;

		return content.ToString();
	}
}
