using SignalDeck.Core.Models;

namespace SignalDeck.Core.Interfaces;

public interface IAudioPlaybackService
{
    Task<PlaybackAttemptResult> PlayAsync(PlaybackSettings playbackSettings, CancellationToken cancellationToken = default);
}
