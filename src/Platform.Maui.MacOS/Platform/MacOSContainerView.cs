using CoreGraphics;
using Microsoft.Maui.Graphics;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS;

public class MacOSContainerView : NSView
{
    public Func<double, double, Graphics.Size>? CrossPlatformMeasure { get; set; }
    public Func<Graphics.Rect, Graphics.Size>? CrossPlatformArrange { get; set; }

    public MacOSContainerView()
    {
        WantsLayer = true;
    }

    // NSView defaults to bottom-left origin; MAUI needs top-left
    public override bool IsFlipped => true;

    /// <summary>
    /// NSView has no SizeThatFits — we provide our own for the base handler to call.
    /// </summary>
    public CGSize SizeThatFits(CGSize size)
    {
        if (CrossPlatformMeasure == null)
            return IntrinsicContentSize;

        var width = double.IsNaN(size.Width) || double.IsInfinity(size.Width)
            ? double.PositiveInfinity
            : (double)size.Width;
        var height = double.IsNaN(size.Height) || double.IsInfinity(size.Height)
            ? double.PositiveInfinity
            : (double)size.Height;

        var result = CrossPlatformMeasure(width, height);
        return new CGSize(result.Width, result.Height);
    }

    public override void Layout()
    {
        base.Layout();

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        // Measure pass must happen before arrange — MAUI's layout engine
        // requires IView.Measure() to be called (which sets DesiredSize) before
        // IView.Arrange() can produce correct results.
        CrossPlatformMeasure?.Invoke((double)bounds.Width, (double)bounds.Height);

        CrossPlatformArrange?.Invoke(new Graphics.Rect(
            0, 0,
            bounds.Width,
            bounds.Height));
    }

    public override CGSize IntrinsicContentSize => new CGSize(NSView.NoIntrinsicMetric, NSView.NoIntrinsicMetric);
}
