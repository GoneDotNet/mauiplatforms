using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Platform.MacOS;

/// <summary>
/// Controls where a toolbar item appears relative to the sidebar tracking separator.
/// </summary>
public enum MacOSToolbarItemPlacement
{
	/// <summary>Standard content area placement (right of the sidebar divider).</summary>
	Content = 0,

	/// <summary>Placed in the sidebar titlebar area (left of the tracking separator).</summary>
	Sidebar = 1,
}

/// <summary>
/// Attached properties for configuring macOS-specific toolbar item behavior.
/// </summary>
public static class MacOSToolbarItem
{
	/// <summary>
	/// Controls whether this toolbar item appears in the sidebar titlebar area
	/// or the standard content toolbar area.
	/// </summary>
	public static readonly BindableProperty PlacementProperty =
		BindableProperty.CreateAttached(
			"Placement",
			typeof(MacOSToolbarItemPlacement),
			typeof(MacOSToolbarItem),
			MacOSToolbarItemPlacement.Content);

	public static MacOSToolbarItemPlacement GetPlacement(BindableObject obj)
		=> (MacOSToolbarItemPlacement)obj.GetValue(PlacementProperty);

	public static void SetPlacement(BindableObject obj, MacOSToolbarItemPlacement value)
		=> obj.SetValue(PlacementProperty, value);
}
