using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class ActivityIndicatorHandler : TvOSViewHandler<IActivityIndicator, UIActivityIndicatorView>
{
    public static readonly IPropertyMapper<IActivityIndicator, ActivityIndicatorHandler> Mapper =
        new PropertyMapper<IActivityIndicator, ActivityIndicatorHandler>(ViewMapper)
        {
            [nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
            [nameof(IActivityIndicator.Color)] = MapColor,
        };

    public ActivityIndicatorHandler() : base(Mapper)
    {
    }

    protected override UIActivityIndicatorView CreatePlatformView()
    {
        return new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Large)
        {
            HidesWhenStopped = true,
        };
    }

    public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (activityIndicator.IsRunning)
            handler.PlatformView.StartAnimating();
        else
            handler.PlatformView.StopAnimating();
    }

    public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (activityIndicator.Color != null)
            handler.PlatformView.Color = activityIndicator.Color.ToPlatformColor();
    }
}
