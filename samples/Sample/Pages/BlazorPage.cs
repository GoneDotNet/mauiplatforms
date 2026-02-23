#if MACAPP
using Microsoft.Maui.Platform.MacOS;
using Microsoft.Maui.Platform.MacOS.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Sample.Pages;

public class BlazorPage : ContentPage
{
	public BlazorPage()
	{
		Title = "Blazor Hybrid";

		var blazorWebView = new MacOSBlazorWebView
		{
			HostPage = "wwwroot/index.html",
			ContentInsets = new Thickness(0, 52, 0, 0),
		};
		blazorWebView.RootComponents.Add(new BlazorRootComponent
		{
			Selector = "#app",
			ComponentType = typeof(SampleMac.Components.Counter),
		});

		Content = blazorWebView;

		// Toolbar group to toggle titlebar separator style
		var separatorGroup = new MacOSToolbarItemGroup
		{
			Label = "Separator",
			SelectionMode = MacOSToolbarGroupSelectionMode.SelectOne,
			SelectedIndex = 0,
			Segments =
			{
				new MacOSToolbarGroupSegment { Text = "Auto", Icon = "line.3.horizontal" },
				new MacOSToolbarGroupSegment { Text = "None", Icon = "circle.slash" },
				new MacOSToolbarGroupSegment { Text = "Line", Icon = "minus" },
			}
		};
		separatorGroup.SelectionChanged += (s, e) =>
		{
			var window = Application.Current?.Windows?.FirstOrDefault();
			if (window == null) return;

			var style = e.SelectedIndex switch
			{
				1 => MacOSTitlebarSeparatorStyle.None,
				2 => MacOSTitlebarSeparatorStyle.Line,
				_ => MacOSTitlebarSeparatorStyle.Automatic,
			};
			MacOSWindow.SetTitlebarSeparatorStyle(window, style);
		};

		MacOSToolbar.SetItemGroups(this, new List<MacOSToolbarItemGroup> { separatorGroup });
	}
}
#endif
