using AVFoundation;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.Essentials.TvOS;

class TextToSpeechImplementation : ITextToSpeech
{
    readonly Lazy<AVSpeechSynthesizer> _synthesizer = new(() => new AVSpeechSynthesizer());

    // Locale constructor is internal and reflection is not available on tvOS (AOT)
    public Task<IEnumerable<Locale>> GetLocalesAsync() =>
        Task.FromResult(Enumerable.Empty<Locale>());

    public async Task SpeakAsync(string text, SpeechOptions? options = default, CancellationToken cancelToken = default)
    {
        if (string.IsNullOrEmpty(text))
            return;

        using var utterance = CreateUtterance(text, options);
        await SpeakUtterance(utterance, cancelToken);
    }

    static AVSpeechUtterance CreateUtterance(string text, SpeechOptions? options)
    {
        var utterance = new AVSpeechUtterance(text);

        if (options is not null)
        {
            utterance.Voice =
                options.Locale?.Id is not null
                    ? AVSpeechSynthesisVoice.FromIdentifier(options.Locale.Id)
                    : AVSpeechSynthesisVoice.FromLanguage(options.Locale?.Language)
                    ?? AVSpeechSynthesisVoice.FromLanguage(AVSpeechSynthesisVoice.CurrentLanguageCode);

            if (options.Pitch.HasValue)
                utterance.PitchMultiplier = options.Pitch.Value;
            if (options.Volume.HasValue)
                utterance.Volume = options.Volume.Value;
            if (options.Rate.HasValue)
                utterance.Rate = options.Rate.Value;
        }

        return utterance;
    }

    async Task SpeakUtterance(AVSpeechUtterance utterance, CancellationToken cancelToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        var synth = _synthesizer.Value;

        try
        {
            synth.DidFinishSpeechUtterance += OnFinished;
            synth.SpeakUtterance(utterance);

            using (cancelToken.Register(() =>
            {
                synth.StopSpeaking(AVSpeechBoundary.Immediate);
                tcs.TrySetResult(true);
            }))
            {
                await tcs.Task;
            }
        }
        finally
        {
            synth.DidFinishSpeechUtterance -= OnFinished;
        }

        void OnFinished(object? sender, AVSpeechSynthesizerUteranceEventArgs args)
        {
            if (args.Utterance == utterance)
                tcs.TrySetResult(true);
        }
    }
}
