using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
#if MACAPP
using Microsoft.Maui.Platform.MacOS.Controls;
#elif TVAPP
using Microsoft.Maui.Platform.TvOS.Controls;
#endif

namespace Sample.Pages;

public class MapPage : ContentPage
{
    readonly MapView _mapView;
    readonly Label _coordsLabel;

    // Preset locations
    static readonly (string Name, double Lat, double Lon)[] Locations =
    [
        ("Seattle", 47.6062, -122.3321),
        ("San Francisco", 37.7749, -122.4194),
        ("New York", 40.7128, -74.0060),
        ("London", 51.5074, -0.1278),
        ("Tokyo", 35.6762, 139.6503),
        ("Sydney", -33.8688, 151.2093),
    ];

    public MapPage()
    {
        Title = "Map";

        _coordsLabel = new Label
        {
            Text = "Seattle (47.61, -122.33)",
            FontSize = 14,
            TextColor = Colors.Gray,
            HorizontalTextAlignment = TextAlignment.Center,
        };

        _mapView = new MapView
        {
            HeightRequest = 400,
            Latitude = Locations[0].Lat,
            Longitude = Locations[0].Lon,
            LatitudeDelta = 0.05,
            LongitudeDelta = 0.05,
        };

        var locationPicker = new Picker { Title = "Jump to location..." };
        foreach (var loc in Locations)
            locationPicker.Items.Add(loc.Name);

        locationPicker.SelectedIndexChanged += (s, e) =>
        {
            if (locationPicker.SelectedIndex < 0) return;
            var loc = Locations[locationPicker.SelectedIndex];
            _mapView.Latitude = loc.Lat;
            _mapView.Longitude = loc.Lon;
            _coordsLabel.Text = $"{loc.Name} ({loc.Lat:F2}, {loc.Lon:F2})";
        };

#if MACAPP
        var mapTypePicker = new Picker { Title = "Map type..." };
        mapTypePicker.Items.Add("Standard");
        mapTypePicker.Items.Add("Satellite");
        mapTypePicker.Items.Add("Hybrid");
        mapTypePicker.SelectedIndex = 0;
        mapTypePicker.SelectedIndexChanged += (s, e) =>
        {
            _mapView.MapType = mapTypePicker.SelectedIndex switch
            {
                1 => MapType.Satellite,
                2 => MapType.Hybrid,
                _ => MapType.Standard,
            };
        };

        var zoomInButton = new Button { Text = "Zoom In" };
        zoomInButton.Clicked += (s, e) =>
        {
            _mapView.LatitudeDelta = Math.Max(0.001, _mapView.LatitudeDelta / 2);
            _mapView.LongitudeDelta = Math.Max(0.001, _mapView.LongitudeDelta / 2);
        };

        var zoomOutButton = new Button { Text = "Zoom Out" };
        zoomOutButton.Clicked += (s, e) =>
        {
            _mapView.LatitudeDelta = Math.Min(90, _mapView.LatitudeDelta * 2);
            _mapView.LongitudeDelta = Math.Min(180, _mapView.LongitudeDelta * 2);
        };
#endif

        var stack = new VerticalStackLayout
        {
            Spacing = 12,
            Padding = new Thickness(24),
            Children =
            {
                new Label
                {
                    Text = "🗺️ MapView",
                    FontSize = 24,
                    FontAttributes = FontAttributes.Bold,
                },
                new BoxView { HeightRequest = 2, Color = Colors.DodgerBlue },
                locationPicker,
#if MACAPP
                new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children = { mapTypePicker, zoomInButton, zoomOutButton },
                },
#endif
                _coordsLabel,
                _mapView,
            },
        };

        Content = new ScrollView { Content = stack };
    }
}
