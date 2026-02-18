using Foundation;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Essentials.TvOS;

/// <summary>
/// tvOS does not have a system pasteboard (UIPasteboard is unavailable).
/// This implementation provides an in-process clipboard for use within the app.
/// </summary>
class ClipboardImplementation : IClipboard
{
	event EventHandler<EventArgs>? _clipboardContentChanged;
	string? _text;

	public bool HasText => !string.IsNullOrEmpty(_text);

	public Task SetTextAsync(string? text)
	{
		_text = text;
		_clipboardContentChanged?.Invoke(this, EventArgs.Empty);
		return Task.CompletedTask;
	}

	public Task<string?> GetTextAsync()
	{
		return Task.FromResult(_text);
	}

	public event EventHandler<EventArgs> ClipboardContentChanged
	{
		add => _clipboardContentChanged += value;
		remove => _clipboardContentChanged -= value;
	}
}
