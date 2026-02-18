using Foundation;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class SearchBarHandler : TvOSViewHandler<ISearchBar, TvOSSearchBarView>
{
	public static readonly IPropertyMapper<ISearchBar, SearchBarHandler> Mapper =
		new PropertyMapper<ISearchBar, SearchBarHandler>(ViewMapper)
		{
			[nameof(ITextInput.Text)] = MapText,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
			[nameof(ISearchBar.CancelButtonColor)] = MapCancelButtonColor,
			[nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
			[nameof(ITextInput.MaxLength)] = MapMaxLength,
		};

	bool _updating;

	public SearchBarHandler() : base(Mapper)
	{
	}

	protected override TvOSSearchBarView CreatePlatformView()
	{
		return new TvOSSearchBarView();
	}

	protected override void ConnectHandler(TvOSSearchBarView platformView)
	{
		base.ConnectHandler(platformView);
		platformView.TextChanged += OnTextChanged;
		platformView.SearchButtonPressed += OnSearchButtonPressed;
	}

	protected override void DisconnectHandler(TvOSSearchBarView platformView)
	{
		platformView.TextChanged -= OnTextChanged;
		platformView.SearchButtonPressed -= OnSearchButtonPressed;
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
				textInput.Text = PlatformView.Text ?? string.Empty;
		}
		finally
		{
			_updating = false;
		}
	}

	void OnSearchButtonPressed(object? sender, EventArgs e)
	{
		if (VirtualView is ISearchBar searchBar)
			searchBar.SearchButtonPressed();
	}

	public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
	{
		if (handler._updating)
			return;

		if (searchBar is ITextInput textInput)
			handler.PlatformView.Text = textInput.Text ?? string.Empty;
	}

	public static void MapTextColor(SearchBarHandler handler, ISearchBar searchBar)
	{
		if (searchBar is ITextStyle textStyle && textStyle.TextColor != null)
			handler.PlatformView.TextColor = textStyle.TextColor.ToPlatformColor();
	}

	public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
	{
		if (searchBar is IPlaceholder placeholder)
			handler.PlatformView.Placeholder = placeholder.Placeholder ?? string.Empty;
	}

	public static void MapCancelButtonColor(SearchBarHandler handler, ISearchBar searchBar)
	{
		// No cancel button in the custom tvOS search bar
	}

	public static void MapIsReadOnly(SearchBarHandler handler, ISearchBar searchBar)
	{
		if (searchBar is ITextInput textInput)
			handler.PlatformView.IsReadOnly = textInput.IsReadOnly;
	}

	public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
	{
		if (searchBar is ITextInput textInput && textInput.MaxLength >= 0)
		{
			var currentText = handler.PlatformView.Text ?? string.Empty;
			if (currentText.Length > textInput.MaxLength)
				handler.PlatformView.Text = currentText[..textInput.MaxLength];
		}
	}
}
