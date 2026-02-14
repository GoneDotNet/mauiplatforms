using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Sample;

public class FlyoutDemoPage : FlyoutPage
{
    public FlyoutDemoPage()
    {
        Title = "FlyoutPage Demo";
        FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;

        Flyout = new ContentPage
        {
            Title = "Menu",
            BackgroundColor = Color.FromArgb("#1E1E3A"),
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(20),
                    Spacing = 12,
                    Children =
                    {
                        new Label
                        {
                            Text = "Sidebar",
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                        },
                        new BoxView
                        {
                            Color = Color.FromArgb("#4A90E2"),
                            HeightRequest = 2,
                        },
                        CreateMenuButton("Home", Color.FromArgb("#4A90E2")),
                        CreateMenuButton("Settings", Color.FromArgb("#7B68EE")),
                        CreateMenuButton("About", Color.FromArgb("#2ECC71")),
                    },
                },
            },
        };

        Detail = new NavigationPage(new ContentPage
        {
            Title = "Detail",
            BackgroundColor = Color.FromArgb("#1A1A2E"),
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(40),
                Spacing = 20,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "FlyoutPage Detail Area",
                        FontSize = 32,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center,
                    },
                    new Label
                    {
                        Text = "The sidebar is rendered using a native NSSplitView.\nResize it by dragging the divider.",
                        FontSize = 18,
                        TextColor = Color.FromArgb("#AAAAAA"),
                        HorizontalTextAlignment = TextAlignment.Center,
                    },
                },
            },
        });
    }

    static Button CreateMenuButton(string text, Color color) => new()
    {
        Text = text,
        BackgroundColor = color,
        TextColor = Colors.White,
    };
}
