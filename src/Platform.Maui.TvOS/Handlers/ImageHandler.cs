using CoreGraphics;
using Foundation;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS.Handlers;

public partial class ImageHandler : TvOSViewHandler<IImage, UIImageView>
{
    public static readonly IPropertyMapper<IImage, ImageHandler> Mapper =
        new PropertyMapper<IImage, ImageHandler>(ViewMapper)
        {
            [nameof(IImage.Aspect)] = MapAspect,
            [nameof(IImage.IsOpaque)] = MapIsOpaque,
            [nameof(IImageSourcePart.Source)] = MapSource,
        };

    public ImageHandler() : base(Mapper)
    {
    }

    protected override UIImageView CreatePlatformView()
    {
        return new UIImageView
        {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            ClipsToBounds = true,
        };
    }

    public static void MapAspect(ImageHandler handler, IImage image)
    {
        handler.PlatformView.ContentMode = image.Aspect switch
        {
            Aspect.AspectFill => UIViewContentMode.ScaleAspectFill,
            Aspect.Fill => UIViewContentMode.ScaleToFill,
            Aspect.Center => UIViewContentMode.Center,
            _ => UIViewContentMode.ScaleAspectFit,
        };
    }

    public static void MapIsOpaque(ImageHandler handler, IImage image)
    {
        handler.PlatformView.Opaque = image.IsOpaque;
    }

    public static void MapSource(ImageHandler handler, IImage image)
    {
        Console.WriteLine($"[ImageHandler] MapSource called, image type: {image.GetType().Name}");

        if (image is IImageSourcePart imageSourcePart)
        {
            Console.WriteLine($"[ImageHandler] Source type: {imageSourcePart.Source?.GetType().Name ?? "null"}");
            handler.LoadImageSource(imageSourcePart);
        }
        else
        {
            Console.Error.WriteLine($"[ImageHandler] Image does not implement IImageSourcePart");
        }
    }

    void LoadImageSource(IImageSourcePart imageSourcePart)
    {
        var source = imageSourcePart.Source;
        if (source == null)
        {
            Console.WriteLine("[ImageHandler] Source is null, clearing image");
            PlatformView.Image = null;
            imageSourcePart.UpdateIsLoading(false);
            return;
        }

        imageSourcePart.UpdateIsLoading(true);

        if (source is IUriImageSource uriImageSource)
        {
            var uri = uriImageSource.Uri;
            Console.WriteLine($"[ImageHandler] Loading URI image: {uri}");
            if (uri != null)
            {
                LoadFromUri(uri, imageSourcePart);
            }
            else
            {
                PlatformView.Image = null;
                imageSourcePart.UpdateIsLoading(false);
            }
        }
        else if (source is IFileImageSource fileImageSource)
        {
            var fileName = fileImageSource.File;
            Console.WriteLine($"[ImageHandler] Loading file image: {fileName}");
            if (!string.IsNullOrEmpty(fileName))
            {
                var uiImage = UIImage.FromBundle(fileName) ?? UIImage.FromFile(fileName);
                PlatformView.Image = uiImage;
                Console.WriteLine($"[ImageHandler] File image loaded: {uiImage != null}");
            }

            imageSourcePart.UpdateIsLoading(false);
        }
        else
        {
            Console.Error.WriteLine($"[ImageHandler] Unsupported source type: {source.GetType().Name}");
            PlatformView.Image = null;
            imageSourcePart.UpdateIsLoading(false);
        }
    }

    async void LoadFromUri(Uri uri, IImageSourcePart imageSourcePart)
    {
        try
        {
            Console.WriteLine($"[ImageHandler] Starting HTTP download: {uri}");
            using var client = new HttpClient();
            var data = await client.GetByteArrayAsync(uri);
            Console.WriteLine($"[ImageHandler] Downloaded {data.Length} bytes");

            var nsData = NSData.FromArray(data);
            var uiImage = UIImage.LoadFromData(nsData);
            Console.WriteLine($"[ImageHandler] UIImage created: {uiImage != null}, size: {uiImage?.Size}");

            if (PlatformView != null)
            {
                PlatformView.Image = uiImage;
                Console.WriteLine("[ImageHandler] Image set on UIImageView");

                // Force layout update after async image load
                PlatformView.SetNeedsLayout();
                PlatformView.Superview?.SetNeedsLayout();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ImageHandler] Failed to load image from URI: {ex}");
        }
        finally
        {
            imageSourcePart.UpdateIsLoading(false);
        }
    }
}
