using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Sample;

public class CollectionViewPage : ContentPage
{
    public CollectionViewPage()
    {
        BackgroundColor = Color.FromArgb("#1A1A2E");

        var items = new List<string>();
        for (int i = 1; i <= 50; i++)
            items.Add($"Item {i}");

        var backButton = new Button
        {
            Text = "← Back",
            BackgroundColor = Color.FromArgb("#4A90E2"),
            TextColor = Colors.White,
        };
        backButton.Clicked += async (s, e) => await Navigation.PopAsync();

        var cv = new CollectionView
        {
            HeightRequest = 500,
            ItemsSource = items,
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    FontSize = 22,
                    TextColor = Colors.White,
                    Padding = new Thickness(20, 14),
                    BackgroundColor = Color.FromArgb("#2A2A4A"),
                };
                label.SetBinding(Label.TextProperty, ".");
                return label;
            }),
        };

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(60, 40),
            Spacing = 20,
            Children =
            {
                new Label
                {
                    Text = "CollectionView Page",
                    FontSize = 44,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                },
                backButton,
                cv,
            },
        };
    }
}
