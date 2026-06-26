using NAudio.CoreAudioApi;
using NAudio.Wave;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;

namespace SignalDeck.Infrastructure.Audio;

public sealed class NaudioPlaybackService : IAudioPlaybackService
{
    public async Task PlayAsync(PlaybackSettings playbackSettings, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(playbackSettings.AudioFilePath))
        {
            return;
        }

        using var enumerator = new MMDeviceEnumerator();
        MMDevice device;

        try
        {
            device = enumerator.GetDevice(playbackSettings.OutputDeviceId);
        }
        catch
        {
            return;
        }

        using var reader = new AudioFileReader(playbackSettings.AudioFilePath)
        {
            Volume = Math.Clamp(playbackSettings.Volume, 0f, 1f)
        };
        using var output = new WasapiOut(device, AudioClientShareMode.Shared, false, 100);
        var playbackCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        output.Init(reader);
        output.PlaybackStopped += (_, args) =>
        {
            if (args.Exception is not null)
            {
                playbackCompleted.TrySetException(args.Exception);
                return;
            }

            playbackCompleted.TrySetResult(true);
        };
        output.Play();

        using var registration = cancellationToken.Register(() =>
        {
            output.Stop();
            playbackCompleted.TrySetCanceled(cancellationToken);
        });

        await playbackCompleted.Task;
    }
}
