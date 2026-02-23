using AppKit;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Media;
using ObjCRuntime;

namespace Microsoft.Maui.Essentials.MacOS;

class ScreenshotImplementation : IScreenshot
{
	public bool IsCaptureSupported => true;

	public Task<IScreenshotResult> CaptureAsync()
	{
		var window = NSApplication.SharedApplication.KeyWindow
			?? NSApplication.SharedApplication.MainWindow;

		if (window == null)
			throw new InvalidOperationException("No window available to capture.");

		// Capture the window using its CGWindowID
		var windowId = (uint)window.WindowNumber;
		var cgImagePtr = CGWindowListCreateImage(
			CGRect.Null,
			CGWindowListOption.IncludingWindow,
			windowId,
			CGWindowImageOption.BoundsIgnoreFraming);

		if (cgImagePtr == IntPtr.Zero)
			throw new InvalidOperationException("Failed to capture window.");

		var cgImage = Runtime.GetINativeObject<CGImage>(cgImagePtr, owns: true)!;
		var nsImage = new NSImage(cgImage, new CGSize(cgImage.Width, cgImage.Height));
		var tiffData = nsImage.AsTiff();
		if (tiffData == null)
			throw new InvalidOperationException("Failed to convert screenshot.");

		var rep = new NSBitmapImageRep(tiffData);
		var width = (int)rep.PixelsWide;
		var height = (int)rep.PixelsHigh;

		IScreenshotResult result = new MacOSScreenshotResult(rep, width, height);
		return Task.FromResult(result);
	}

	[System.Runtime.InteropServices.DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	static extern IntPtr CGWindowListCreateImage(CGRect screenBounds, CGWindowListOption listOption, uint windowId, CGWindowImageOption imageOption);
}

class MacOSScreenshotResult : IScreenshotResult
{
	readonly NSBitmapImageRep _rep;

	public MacOSScreenshotResult(NSBitmapImageRep rep, int width, int height)
	{
		_rep = rep;
		Width = width;
		Height = height;
	}

	public int Width { get; }
	public int Height { get; }

	public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
	{
		var data = GetImageData(format, quality);
		Stream stream = data.AsStream();
		return Task.FromResult(stream);
	}

	public async Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
	{
		using var stream = await OpenReadAsync(format, quality);
		await stream.CopyToAsync(destination);
	}

	NSData GetImageData(ScreenshotFormat format, int quality)
	{
		if (format == ScreenshotFormat.Jpeg)
		{
			var props = new NSDictionary(
				NSBitmapImageRep.CompressionFactor,
				new NSNumber(quality / 100.0));
			return _rep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Jpeg, props)
				?? throw new InvalidOperationException("Failed to encode JPEG.");
		}

		return _rep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png)
			?? throw new InvalidOperationException("Failed to encode PNG.");
	}
}
