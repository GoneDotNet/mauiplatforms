using Microsoft.Maui.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

public partial class ActivityIndicatorHandler : MacOSViewHandler<IActivityIndicator, NSProgressIndicator>
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

    protected override NSProgressIndicator CreatePlatformView()
    {
        return new NSProgressIndicator
        {
            Style = NSProgressIndicatorStyle.Spinning,
            IsDisplayedWhenStopped = false,
            ControlSize = NSControlSize.Regular,
        };
    }

    public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (activityIndicator.IsRunning)
            handler.PlatformView.StartAnimation(null);
        else
            handler.PlatformView.StopAnimation(null);
    }

    public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (activityIndicator.Color != null)
        {
            handler.PlatformView.WantsLayer = true;
            handler.PlatformView.ContentFilters = [];
            // NSProgressIndicator doesn't directly support color tinting;
            // use a content filter or appearance. For now, use the default.
        }
    }
}
