using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Platform.MacOS;

/// <summary>
/// Controls where a toolbar item appears relative to the sidebar tracking separator.
/// Sidebar placements (Leading/Center/Trailing) are separated by flexible spaces
/// so items distribute across the sidebar titlebar area like:
/// <c>[Leading...] ←flex→ [Center...] ←flex→ [Trailing...]  |  [Content...]</c>
/// These are ignored when an explicit <see cref="MacOSToolbar.SidebarLayoutProperty"/> is set.
/// </summary>
public enum MacOSToolbarItemPlacement
{
	/// <summary>Standard content area placement (right of the sidebar divider).</summary>
	Content = 0,

	/// <summary>Sidebar titlebar, left-aligned (equivalent to <see cref="SidebarLeading"/>).</summary>
	Sidebar = 1,

	/// <summary>Sidebar titlebar, left-aligned.</summary>
	SidebarLeading = 1,

	/// <summary>Sidebar titlebar, centered between flexible spaces.</summary>
	SidebarCenter = 2,

	/// <summary>Sidebar titlebar, right-aligned (pushed right by a flexible space).</summary>
	SidebarTrailing = 3,
}

/// <summary>
/// Attached properties for configuring macOS-specific toolbar item behavior.
/// </summary>
public static class MacOSToolbarItem
{
	/// <summary>
	/// Controls where this toolbar item appears: in the content toolbar area,
	/// or in the sidebar titlebar area (leading, center, or trailing).
	/// Ignored when <see cref="MacOSToolbar.SidebarLayoutProperty"/> is set on the page.
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

/// <summary>
/// Describes a single element in an explicit sidebar toolbar layout.
/// Use the static factory members to build a layout array.
/// </summary>
/// <example>
/// <code>
/// MacOSToolbar.SetSidebarLayout(page, new[]
/// {
///     MacOSToolbarLayoutItem.Item(addBtn),
///     MacOSToolbarLayoutItem.FlexibleSpace,
///     MacOSToolbarLayoutItem.Item(filterBtn),
/// });
/// </code>
/// </example>
public abstract class MacOSToolbarLayoutItem
{
	/// <summary>A spring that pushes adjacent items apart.</summary>
	public static readonly MacOSToolbarLayoutItem FlexibleSpace = new SpacerLayoutItem(SpacerKind.Flexible);

	/// <summary>A fixed-width space between items.</summary>
	public static readonly MacOSToolbarLayoutItem Space = new SpacerLayoutItem(SpacerKind.Fixed);

	/// <summary>A thin vertical separator line.</summary>
	public static readonly MacOSToolbarLayoutItem Separator = new SpacerLayoutItem(SpacerKind.Separator);

	/// <summary>The page title label (centered by default). Use in content layouts to position the title.</summary>
	public static readonly MacOSToolbarLayoutItem Title = new TitleLayoutItem();

	/// <summary>References a <see cref="ToolbarItem"/> that must also be in the page's ToolbarItems collection.</summary>
	public static MacOSToolbarLayoutItem Item(ToolbarItem item) => new ToolbarItemLayoutRef(item);
}

/// <summary>The kind of spacer in a toolbar layout.</summary>
public enum SpacerKind { Flexible, Fixed, Separator }

/// <summary>A spacer or separator in the toolbar layout.</summary>
public sealed class SpacerLayoutItem : MacOSToolbarLayoutItem
{
	public SpacerKind Kind { get; }
	internal SpacerLayoutItem(SpacerKind kind) => Kind = kind;
}

/// <summary>A reference to a <see cref="ToolbarItem"/> in the toolbar layout.</summary>
public sealed class ToolbarItemLayoutRef : MacOSToolbarLayoutItem
{
	public ToolbarItem ToolbarItem { get; }
	internal ToolbarItemLayoutRef(ToolbarItem item) => ToolbarItem = item;
}

/// <summary>The page title element in a content toolbar layout.</summary>
public sealed class TitleLayoutItem : MacOSToolbarLayoutItem
{
	internal TitleLayoutItem() { }
}

/// <summary>
/// Attached properties for configuring the macOS toolbar layout at the page level.
/// </summary>
public static class MacOSToolbar
{
	/// <summary>
	/// When set on a <see cref="Page"/>, defines the exact layout of the sidebar toolbar area.
	/// Overrides the per-item <see cref="MacOSToolbarItem.PlacementProperty"/> convenience API.
	/// Items referenced here must also be in <see cref="Page.ToolbarItems"/>.
	/// Items NOT in this layout go to the content toolbar area (or <see cref="ContentLayoutProperty"/>).
	/// </summary>
	public static readonly BindableProperty SidebarLayoutProperty =
		BindableProperty.CreateAttached(
			"SidebarLayout",
			typeof(IList<MacOSToolbarLayoutItem>),
			typeof(MacOSToolbar),
			defaultValue: null);

	public static IList<MacOSToolbarLayoutItem>? GetSidebarLayout(BindableObject obj)
		=> (IList<MacOSToolbarLayoutItem>?)obj.GetValue(SidebarLayoutProperty);

	public static void SetSidebarLayout(BindableObject obj, IList<MacOSToolbarLayoutItem>? value)
		=> obj.SetValue(SidebarLayoutProperty, value);

	/// <summary>
	/// When set on a <see cref="Page"/>, defines the exact layout of the content toolbar area
	/// (right of the tracking separator). Items referenced here must also be in
	/// <see cref="Page.ToolbarItems"/>. When not set, content items are laid out with
	/// the default [flex] [title] [flex] [items...] pattern.
	/// </summary>
	public static readonly BindableProperty ContentLayoutProperty =
		BindableProperty.CreateAttached(
			"ContentLayout",
			typeof(IList<MacOSToolbarLayoutItem>),
			typeof(MacOSToolbar),
			defaultValue: null);

	public static IList<MacOSToolbarLayoutItem>? GetContentLayout(BindableObject obj)
		=> (IList<MacOSToolbarLayoutItem>?)obj.GetValue(ContentLayoutProperty);

	public static void SetContentLayout(BindableObject obj, IList<MacOSToolbarLayoutItem>? value)
		=> obj.SetValue(ContentLayoutProperty, value);
}
