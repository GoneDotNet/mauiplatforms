using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Platform.TvOS.Hosting;

public class TvOSMauiContext : IMauiContext
{
    readonly IServiceProvider _services;
    IMauiHandlersFactory? _handlers;

    public TvOSMauiContext(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    TvOSMauiContext(IServiceProvider services, IMauiHandlersFactory handlers)
    {
        _services = services;
        _handlers = handlers;
    }

    public IServiceProvider Services => _services;

    public IMauiHandlersFactory Handlers =>
        _handlers ??= _services.GetRequiredService<IMauiHandlersFactory>();

    public TvOSMauiContext MakeWindowScope(IServiceProvider windowServices)
    {
        return new TvOSMauiContext(windowServices, Handlers);
    }
}
