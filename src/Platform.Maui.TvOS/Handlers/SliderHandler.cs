using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class SliderHandler : TvOSViewHandler<ISlider, TvOSSliderView>
{
    public static readonly IPropertyMapper<ISlider, SliderHandler> Mapper =
        new PropertyMapper<ISlider, SliderHandler>(ViewMapper)
        {
            [nameof(IRange.Minimum)] = MapMinimum,
            [nameof(IRange.Maximum)] = MapMaximum,
            [nameof(IRange.Value)] = MapValue,
            [nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
            [nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
            [nameof(ISlider.ThumbColor)] = MapThumbColor,
        };

    bool _updating;

    public SliderHandler() : base(Mapper)
    {
    }

    protected override TvOSSliderView CreatePlatformView()
    {
        return new TvOSSliderView();
    }

    protected override void ConnectHandler(TvOSSliderView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ValueChanged += OnValueChanged;
        platformView.DragStarted += OnDragStarted;
        platformView.DragCompleted += OnDragCompleted;
    }

    protected override void DisconnectHandler(TvOSSliderView platformView)
    {
        platformView.ValueChanged -= OnValueChanged;
        platformView.DragStarted -= OnDragStarted;
        platformView.DragCompleted -= OnDragCompleted;
        base.DisconnectHandler(platformView);
    }

    void OnValueChanged(object? sender, EventArgs e)
    {
        if (_updating || VirtualView == null)
            return;

        _updating = true;
        try
        {
            if (VirtualView is IRange range)
                range.Value = PlatformView.Value;
        }
        finally
        {
            _updating = false;
        }
    }

    void OnDragStarted(object? sender, EventArgs e)
    {
        VirtualView?.DragStarted();
    }

    void OnDragCompleted(object? sender, EventArgs e)
    {
        VirtualView?.DragCompleted();
    }

    public static void MapMinimum(SliderHandler handler, ISlider slider)
    {
        if (slider is IRange range)
            handler.PlatformView.Minimum = range.Minimum;
    }

    public static void MapMaximum(SliderHandler handler, ISlider slider)
    {
        if (slider is IRange range)
            handler.PlatformView.Maximum = range.Maximum;
    }

    public static void MapValue(SliderHandler handler, ISlider slider)
    {
        if (handler._updating)
            return;

        if (slider is IRange range)
            handler.PlatformView.Value = range.Value;
    }

    public static void MapMinimumTrackColor(SliderHandler handler, ISlider slider)
    {
        handler.PlatformView.MinimumTrackTintColor = slider.MinimumTrackColor?.ToPlatformColor();
    }

    public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
    {
        handler.PlatformView.MaximumTrackTintColor = slider.MaximumTrackColor?.ToPlatformColor();
    }

    public static void MapThumbColor(SliderHandler handler, ISlider slider)
    {
        handler.PlatformView.ThumbTintColor = slider.ThumbColor?.ToPlatformColor();
    }
}
