using CoreLocation;
using MapKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform.TvOS.Controls;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class MapViewHandler : TvOSViewHandler<MapView, MKMapView>
{
    public static readonly IPropertyMapper<MapView, MapViewHandler> Mapper =
        new PropertyMapper<MapView, MapViewHandler>(ViewMapper)
        {
            [nameof(MapView.Latitude)] = MapRegion,
            [nameof(MapView.Longitude)] = MapRegion,
            [nameof(MapView.LatitudeDelta)] = MapRegion,
            [nameof(MapView.LongitudeDelta)] = MapRegion,
            [nameof(MapView.MapType)] = MapMapType,
            [nameof(MapView.IsShowingUser)] = MapIsShowingUser,
        };

    public MapViewHandler() : base(Mapper)
    {
    }

    protected override MKMapView CreatePlatformView()
    {
        return new MKMapView();
    }

    protected override void ConnectHandler(MKMapView platformView)
    {
        base.ConnectHandler(platformView);
        UpdateRegion();
    }

    public static void MapRegion(MapViewHandler handler, MapView mapView)
    {
        handler.UpdateRegion();
    }

    void UpdateRegion()
    {
        var center = new CLLocationCoordinate2D(VirtualView.Latitude, VirtualView.Longitude);
        var span = new MKCoordinateSpan(VirtualView.LatitudeDelta, VirtualView.LongitudeDelta);
        var region = new MKCoordinateRegion(center, span);
        PlatformView.SetRegion(region, animated: true);
    }

    public static void MapMapType(MapViewHandler handler, MapView mapView)
    {
        handler.PlatformView.MapType = mapView.MapType switch
        {
            Controls.MapType.Satellite => MKMapType.Satellite,
            Controls.MapType.Hybrid => MKMapType.Hybrid,
            _ => MKMapType.Standard,
        };
    }

    public static void MapIsShowingUser(MapViewHandler handler, MapView mapView)
    {
        handler.PlatformView.ShowsUserLocation = mapView.IsShowingUser;
    }

    public override Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var width = double.IsPositiveInfinity(widthConstraint) ? 400 : widthConstraint;
        var height = double.IsPositiveInfinity(heightConstraint) ? 400 : heightConstraint;
        return new Graphics.Size(width, height);
    }
}
