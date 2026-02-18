using CoreFoundation;
using Foundation;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Platform.TvOS.Dispatching;

public class TvOSDispatcher : IDispatcher
{
    public static IDispatcher? GetForCurrentThread()
    {
        if (NSThread.IsMain)
            return new TvOSDispatcher();
        return null;
    }

    public bool IsDispatchRequired => !NSThread.IsMain;

    public IDispatcherTimer CreateTimer()
    {
        return new TvOSDispatcherTimer();
    }

    public bool Dispatch(Action action)
    {
        DispatchQueue.MainQueue.DispatchAsync(action);
        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action)
    {
        DispatchQueue.MainQueue.DispatchAfter(
            new DispatchTime(DispatchTime.Now, delay),
            action);
        return true;
    }
}

public class TvOSDispatcherProvider : IDispatcherProvider
{
    public IDispatcher? GetForCurrentThread() => TvOSDispatcher.GetForCurrentThread();
}
