using Foundation;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class ApplicationHandler : ElementHandler<IApplication, NSObject>
{
    public static readonly IPropertyMapper<IApplication, ApplicationHandler> Mapper =
        new PropertyMapper<IApplication, ApplicationHandler>(ElementMapper)
        {
        };

    public static readonly CommandMapper<IApplication, ApplicationHandler> CommandMapper =
        new(ElementCommandMapper)
        {
            [nameof(IApplication.OpenWindow)] = MapOpenWindow,
            [nameof(IApplication.CloseWindow)] = MapCloseWindow,
        };

    public ApplicationHandler() : base(Mapper, CommandMapper)
    {
    }

    protected override NSObject CreatePlatformElement()
    {
        return new NSObject();
    }

    public static void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
    {
        // tvOS is single-window, no-op for additional windows
    }

    public static void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
    {
        // tvOS is single-window, no-op
    }
}
