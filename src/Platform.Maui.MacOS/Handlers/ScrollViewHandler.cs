using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AppKit;

namespace Microsoft.Maui.Platform.MacOS.Handlers;

/// <summary>
/// Flipped NSView used as NSScrollView.DocumentView for correct top-left origin.
/// </summary>
internal class FlippedDocumentView : NSView
{
    public FlippedDocumentView()
    {
        WantsLayer = true;
    }

    public override bool IsFlipped => true;
}

public partial class ScrollViewHandler : MacOSViewHandler<IScrollView, NSScrollView>
{
    public static readonly IPropertyMapper<IScrollView, ScrollViewHandler> Mapper =
        new PropertyMapper<IScrollView, ScrollViewHandler>(ViewMapper)
        {
            [nameof(IScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
            [nameof(IScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
            [nameof(IScrollView.Orientation)] = MapOrientation,
            [nameof(IScrollView.ContentSize)] = MapContentSize,
            [nameof(IContentView.Content)] = MapContent,
        };

    FlippedDocumentView? _documentView;

    public ScrollViewHandler() : base(Mapper)
    {
    }

    protected override NSScrollView CreatePlatformView()
    {
        var scrollView = new NSScrollView
        {
            HasVerticalScroller = true,
            HasHorizontalScroller = false,
            AutohidesScrollers = true,
            DrawsBackground = false,
        };

        _documentView = new FlippedDocumentView();
        scrollView.DocumentView = _documentView;

        return scrollView;
    }

    public override void PlatformArrange(Rect rect)
    {
        base.PlatformArrange(rect);

        if (VirtualView?.PresentedContent is IView content && content.Handler != null && _documentView != null)
        {
            var orientation = VirtualView.Orientation;

            // Measure the content with appropriate constraints based on scroll orientation
            double measureWidth = orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both
                ? double.PositiveInfinity
                : rect.Width;

            double measureHeight = orientation == ScrollOrientation.Vertical || orientation == ScrollOrientation.Both
                ? double.PositiveInfinity
                : rect.Height;

            var contentSize = content.Measure(measureWidth, measureHeight);

            var arrangeWidth = orientation == ScrollOrientation.Vertical
                ? rect.Width
                : Math.Max(rect.Width, contentSize.Width);

            var arrangeHeight = orientation == ScrollOrientation.Horizontal
                ? rect.Height
                : Math.Max(rect.Height, contentSize.Height);

            content.Arrange(new Rect(0, 0, arrangeWidth, arrangeHeight));
            _documentView.Frame = new CGRect(0, 0, arrangeWidth, arrangeHeight);
        }
    }

    public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
    {
        if (handler.PlatformView == null || handler.MauiContext == null || handler._documentView == null)
            return;

        // Clear existing content from document view
        foreach (var subview in handler._documentView.Subviews)
            subview.RemoveFromSuperview();

        if (scrollView.PresentedContent is IView content)
        {
            var platformView = content.ToMacOSPlatform(handler.MauiContext);
            handler._documentView.AddSubview(platformView);
        }
    }

    public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
    {
        handler.PlatformView.HasHorizontalScroller = scrollView.HorizontalScrollBarVisibility != ScrollBarVisibility.Never;
    }

    public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
    {
        handler.PlatformView.HasVerticalScroller = scrollView.VerticalScrollBarVisibility != ScrollBarVisibility.Never;
    }

    public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView)
    {
        var orientation = scrollView.Orientation;
        handler.PlatformView.HasHorizontalScroller =
            orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both;
        handler.PlatformView.HasVerticalScroller =
            orientation == ScrollOrientation.Vertical || orientation == ScrollOrientation.Both;
    }

    public static void MapContentSize(ScrollViewHandler handler, IScrollView scrollView)
    {
        if (handler._documentView != null)
        {
            var contentSize = scrollView.ContentSize;
            handler._documentView.Frame = new CGRect(0, 0, contentSize.Width, contentSize.Height);
        }
    }
}
