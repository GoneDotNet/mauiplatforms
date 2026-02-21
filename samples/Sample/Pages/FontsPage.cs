using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Sample.Pages;

public class FontsPage : ContentPage
{
	public FontsPage()
	{
		Title = "Fonts";

		var stack = new VerticalStackLayout { Spacing = 15, Padding = 20 };

		// System font
		stack.Children.Add(new Label
		{
			Text = "System Font (Default)",
			FontSize = 16,
		});

		// Embedded font by alias
		stack.Children.Add(new Label
		{
			Text = "OpenSans Regular (Embedded Font via alias)",
			FontFamily = "OpenSansRegular",
			FontSize = 16,
		});

		// Embedded font by filename
		stack.Children.Add(new Label
		{
			Text = "OpenSans Regular (Embedded Font via filename)",
			FontFamily = "OpenSans-Regular",
			FontSize = 16,
		});

		// Bold system font
		stack.Children.Add(new Label
		{
			Text = "System Font Bold",
			FontSize = 16,
			FontAttributes = FontAttributes.Bold,
		});

		// Italic system font
		stack.Children.Add(new Label
		{
			Text = "System Font Italic",
			FontSize = 16,
			FontAttributes = FontAttributes.Italic,
		});

		// Various sizes with embedded font
		stack.Children.Add(new Label
		{
			Text = "OpenSans Size 10",
			FontFamily = "OpenSansRegular",
			FontSize = 10,
		});
		stack.Children.Add(new Label
		{
			Text = "OpenSans Size 18",
			FontFamily = "OpenSansRegular",
			FontSize = 18,
		});
		stack.Children.Add(new Label
		{
			Text = "OpenSans Size 24",
			FontFamily = "OpenSansRegular",
			FontSize = 24,
		});

		// Named font by macOS system name
		stack.Children.Add(new Label
		{
			Text = "Menlo (macOS system font)",
			FontFamily = "Menlo",
			FontSize = 14,
		});

		stack.Children.Add(new Label
		{
			Text = "Georgia (macOS system font)",
			FontFamily = "Georgia",
			FontSize = 14,
		});

		// Button with embedded font
		stack.Children.Add(new Button
		{
			Text = "Button with OpenSans",
			FontFamily = "OpenSansRegular",
			FontSize = 14,
		});

		// Entry with embedded font
		stack.Children.Add(new Entry
		{
			Placeholder = "Entry with OpenSans",
			FontFamily = "OpenSansRegular",
			FontSize = 14,
		});

		// --- FontImageSource section ---
		stack.Children.Add(new Label
		{
			Text = "FontImageSource (Font Icons)",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.CornflowerBlue,
			Margin = new Thickness(0, 16, 0, 0),
		});

		// Unicode glyph from system font
		stack.Children.Add(new HorizontalStackLayout
		{
			Spacing = 12,
			Children =
			{
				new Image
				{
					Source = new FontImageSource { Glyph = "★", Color = Colors.Gold, Size = 32 },
					WidthRequest = 32, HeightRequest = 32,
				},
				new Image
				{
					Source = new FontImageSource { Glyph = "♥", Color = Colors.Red, Size = 32 },
					WidthRequest = 32, HeightRequest = 32,
				},
				new Image
				{
					Source = new FontImageSource { Glyph = "⚡", Color = Colors.Orange, Size = 32 },
					WidthRequest = 32, HeightRequest = 32,
				},
				new Image
				{
					Source = new FontImageSource { Glyph = "✓", Color = Colors.Green, Size = 32 },
					WidthRequest = 32, HeightRequest = 32,
				},
				new Image
				{
					Source = new FontImageSource { Glyph = "⚙", Color = Colors.Gray, Size = 32 },
					WidthRequest = 32, HeightRequest = 32,
				},
				new Label { Text = "Unicode glyphs", VerticalTextAlignment = TextAlignment.Center },
			}
		});

		// FontImageSource on a Button
		stack.Children.Add(new Button
		{
			Text = "Button with Font Icon",
			ImageSource = new FontImageSource { Glyph = "⬇", Color = Colors.White, Size = 18 },
		});

		Content = new ScrollView { Content = stack };
	}
}
