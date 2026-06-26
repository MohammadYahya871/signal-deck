using SignalDeck.Core.Interfaces;
using Windows.Media.Control;

namespace SignalDeck.Infrastructure.Idle;

public sealed class WindowsMediaSessionPlaybackMonitor : IPlaybackActivityMonitor
{
    private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;

    public bool IsPlaybackActive()
    {
        try
        {
            _sessionManager ??= GlobalSystemMediaTransportControlsSessionManager
                .RequestAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();

            foreach (var session in _sessionManager.GetSessions())
            {
                var playbackInfo = session.GetPlaybackInfo();
                if (playbackInfo is null)
                {
                    continue;
                }

                if (playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                {
                    return true;
                }
            }
        }
        catch
        {
            // If media-session state cannot be read, fall back to other playback detectors.
        }

        return false;
    }
}
