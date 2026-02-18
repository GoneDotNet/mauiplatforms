using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Platform.TvOS.Controls;

public enum MapType
{
    Standard,
    Satellite,
    Hybrid,
}

/// <summary>
/// A MapView control for tvOS that displays an MKMapView (display-only, no user interaction).
/// </summary>
public class MapView : View
{
    public static readonly BindableProperty LatitudeProperty =
        BindableProperty.Create(nameof(Latitude), typeof(double), typeof(MapView), 47.6062);

    public static readonly BindableProperty LongitudeProperty =
        BindableProperty.Create(nameof(Longitude), typeof(double), typeof(MapView), -122.3321);

    public static readonly BindableProperty LatitudeDeltaProperty =
        BindableProperty.Create(nameof(LatitudeDelta), typeof(double), typeof(MapView), 0.05);

    public static readonly BindableProperty LongitudeDeltaProperty =
        BindableProperty.Create(nameof(LongitudeDelta), typeof(double), typeof(MapView), 0.05);

    public static readonly BindableProperty MapTypeProperty =
        BindableProperty.Create(nameof(MapType), typeof(MapType), typeof(MapView), MapType.Standard);

    public static readonly BindableProperty IsShowingUserProperty =
        BindableProperty.Create(nameof(IsShowingUser), typeof(bool), typeof(MapView), false);

    public double Latitude
    {
        get => (double)GetValue(LatitudeProperty);
        set => SetValue(LatitudeProperty, value);
    }

    public double Longitude
    {
        get => (double)GetValue(LongitudeProperty);
        set => SetValue(LongitudeProperty, value);
    }

    public double LatitudeDelta
    {
        get => (double)GetValue(LatitudeDeltaProperty);
        set => SetValue(LatitudeDeltaProperty, value);
    }

    public double LongitudeDelta
    {
        get => (double)GetValue(LongitudeDeltaProperty);
        set => SetValue(LongitudeDeltaProperty, value);
    }

    public MapType MapType
    {
        get => (MapType)GetValue(MapTypeProperty);
        set => SetValue(MapTypeProperty, value);
    }

    public bool IsShowingUser
    {
        get => (bool)GetValue(IsShowingUserProperty);
        set => SetValue(IsShowingUserProperty, value);
    }
}
