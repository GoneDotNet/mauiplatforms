using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class ShapeViewHandler : TvOSViewHandler<IShapeView, UIView>
{
    public static readonly IPropertyMapper<IShapeView, ShapeViewHandler> Mapper =
        new PropertyMapper<IShapeView, ShapeViewHandler>(ViewMapper)
        {
            [nameof(IShapeView.Shape)] = MapShape,
            [nameof(IShapeView.Fill)] = MapFill,
            [nameof(IShapeView.Aspect)] = MapAspect,
        };

    CAShapeLayer? _shapeLayer;

    public ShapeViewHandler() : base(Mapper)
    {
    }

    protected override UIView CreatePlatformView()
    {
        return new UIView
        {
            ClipsToBounds = true,
        };
    }

    public static void MapFill(ShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler._shapeLayer != null)
        {
            if (shapeView.Fill is SolidPaint solidPaint && solidPaint.Color != null)
                handler._shapeLayer.FillColor = solidPaint.Color.ToPlatformColor().CGColor;
            else
                handler._shapeLayer.FillColor = UIColor.Clear.CGColor;
        }
        else
        {
            if (shapeView.Fill is SolidPaint solidPaint && solidPaint.Color != null)
                handler.PlatformView.BackgroundColor = solidPaint.Color.ToPlatformColor();
            else
                handler.PlatformView.BackgroundColor = UIColor.Clear;
        }
    }

    public static void MapShape(ShapeViewHandler handler, IShapeView shapeView)
    {
        handler.UpdateShape(shapeView);
    }

    public static void MapAspect(ShapeViewHandler handler, IShapeView shapeView)
    {
        handler.UpdateShape(shapeView);
    }

    void UpdateShape(IShapeView shapeView)
    {
        var bounds = PlatformView.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var shape = shapeView.Shape;
        if (shape == null)
        {
            _shapeLayer?.RemoveFromSuperLayer();
            _shapeLayer = null;
            return;
        }

        var pathBounds = new Graphics.Rect(0, 0, (double)bounds.Width, (double)bounds.Height);
        var pathF = shape.PathForBounds(pathBounds);
        if (pathF == null)
            return;

        if (_shapeLayer == null)
        {
            _shapeLayer = new CAShapeLayer();
            PlatformView.Layer.AddSublayer(_shapeLayer);
            PlatformView.BackgroundColor = UIColor.Clear;
        }

        _shapeLayer.Frame = bounds;
        _shapeLayer.Path = PathFToCGPath(pathF);

        if (shapeView.Fill is SolidPaint solidPaint && solidPaint.Color != null)
            _shapeLayer.FillColor = solidPaint.Color.ToPlatformColor().CGColor;
        else
            _shapeLayer.FillColor = UIColor.Clear.CGColor;
    }

    static CGPath PathFToCGPath(PathF pathF)
    {
        var cgPath = new CGPath();

        var points = pathF.Points?.ToArray();
        if (points == null || points.Length == 0)
            return cgPath;

        var segments = pathF.SegmentTypes?.ToArray();
        if (segments == null || segments.Length == 0)
            return cgPath;

        // Walk through path operations
        int index = 0;
        foreach (var op in segments)
        {
            switch (op)
            {
                case PathOperation.Move:
                    if (index < points.Length)
                    {
                        cgPath.MoveToPoint(points[index].X, points[index].Y);
                        index++;
                    }
                    break;

                case PathOperation.Line:
                    if (index < points.Length)
                    {
                        cgPath.AddLineToPoint(points[index].X, points[index].Y);
                        index++;
                    }
                    break;

                case PathOperation.Quad:
                    if (index + 1 < points.Length)
                    {
                        cgPath.AddQuadCurveToPoint(
                            points[index].X, points[index].Y,
                            points[index + 1].X, points[index + 1].Y);
                        index += 2;
                    }
                    break;

                case PathOperation.Cubic:
                    if (index + 2 < points.Length)
                    {
                        cgPath.AddCurveToPoint(
                            points[index].X, points[index].Y,
                            points[index + 1].X, points[index + 1].Y,
                            points[index + 2].X, points[index + 2].Y);
                        index += 3;
                    }
                    break;

                case PathOperation.Close:
                    cgPath.CloseSubpath();
                    break;
            }
        }

        return cgPath;
    }

    public override void PlatformArrange(Graphics.Rect rect)
    {
        base.PlatformArrange(rect);

        if (VirtualView != null && rect.Width > 0 && rect.Height > 0)
            UpdateShape(VirtualView);
    }
}
