using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class ScrollViewHandler : TvOSViewHandler<IScrollView, UIScrollView>
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

    public ScrollViewHandler() : base(Mapper)
    {
    }

    protected override UIScrollView CreatePlatformView()
    {
        return new UIScrollView();
    }

    protected override void ConnectHandler(UIScrollView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Scrolled += OnScrolled;
        platformView.DecelerationEnded += OnDecelerationEnded;
    }

    protected override void DisconnectHandler(UIScrollView platformView)
    {
        platformView.Scrolled -= OnScrolled;
        platformView.DecelerationEnded -= OnDecelerationEnded;
        base.DisconnectHandler(platformView);
    }

    void OnScrolled(object? sender, EventArgs e)
    {
        // Scroll offset updates happen continuously during scrolling
    }

    void OnDecelerationEnded(object? sender, EventArgs e)
    {
        VirtualView?.ScrollFinished();
    }

    public override void PlatformArrange(Rect rect)
    {
        base.PlatformArrange(rect);

        if (VirtualView?.PresentedContent is IView content && content.Handler != null)
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
            PlatformView.ContentSize = new CGSize(arrangeWidth, arrangeHeight);
        }
    }

    public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
    {
        if (handler.PlatformView == null || handler.MauiContext == null)
            return;

        // Clear existing content
        foreach (var subview in handler.PlatformView.Subviews)
            subview.RemoveFromSuperview();

        if (scrollView.PresentedContent is IView content)
        {
            var platformView = content.ToTvOSPlatform(handler.MauiContext);
            handler.PlatformView.AddSubview(platformView);
        }
    }

    public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
    {
        handler.PlatformView.ShowsHorizontalScrollIndicator = scrollView.HorizontalScrollBarVisibility != ScrollBarVisibility.Never;
    }

    public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
    {
        handler.PlatformView.ShowsVerticalScrollIndicator = scrollView.VerticalScrollBarVisibility != ScrollBarVisibility.Never;
    }

    public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView)
    {
        var orientation = scrollView.Orientation;
        handler.PlatformView.AlwaysBounceHorizontal =
            orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both;
        handler.PlatformView.AlwaysBounceVertical =
            orientation == ScrollOrientation.Vertical || orientation == ScrollOrientation.Both;
    }

    public static void MapContentSize(ScrollViewHandler handler, IScrollView scrollView)
    {
        var contentSize = scrollView.ContentSize;
        handler.PlatformView.ContentSize = new CGSize(contentSize.Width, contentSize.Height);
    }
}
