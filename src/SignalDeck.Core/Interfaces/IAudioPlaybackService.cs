using SignalDeck.Core.Models;

namespace SignalDeck.Core.Interfaces;

public interface IAudioPlaybackService
{
    Task PlayAsync(PlaybackSettings playbackSettings, CancellationToken cancellationToken = default);
}
