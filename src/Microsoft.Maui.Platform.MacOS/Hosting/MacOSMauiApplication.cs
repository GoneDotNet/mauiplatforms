using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform.MacOS.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Hosting;

[Register("MacOSMauiApplication")]
public abstract class MacOSMauiApplication : NSApplicationDelegate, IPlatformApplication
{
    IApplication _mauiApp = null!;

    public IServiceProvider Services { get; protected set; } = null!;

    public IApplication Application => _mauiApp;

    protected abstract MauiApp CreateMauiApp();

    public override void DidFinishLaunching(NSNotification notification)
    {
        try
        {
            IPlatformApplication.Current = this;

            var mauiApp = CreateMauiApp();

            var rootContext = new MacOSMauiContext(mauiApp.Services);
            var applicationContext = rootContext.MakeApplicationScope(this);

            Services = applicationContext.Services;

            _mauiApp = Services.GetRequiredService<IApplication>();

            // Wire up ApplicationHandler
            var appHandler = new ApplicationHandler();
            appHandler.SetMauiContext(applicationContext);
            appHandler.SetVirtualView(_mauiApp);

            // Create the window
            CreatePlatformWindow(applicationContext);

            OnStarted();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"MAUI STARTUP EXCEPTION: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Called after the MAUI application and window have been fully initialized.
    /// Override to perform post-startup actions like starting debug agents.
    /// </summary>
    protected virtual void OnStarted() { }

    private void CreatePlatformWindow(MacOSMauiContext applicationContext)
    {
        var virtualWindow = _mauiApp.CreateWindow(null);

        var windowContext = applicationContext.MakeWindowScope(new NSWindow());

        var windowHandler = new WindowHandler();
        windowHandler.SetMauiContext(windowContext);
        windowHandler.SetVirtualView(virtualWindow);

        virtualWindow.Created();
        virtualWindow.Activated();
    }
}
