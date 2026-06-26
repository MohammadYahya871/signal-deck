using SignalDeck.Core.Models;

namespace SignalDeck.Core.Interfaces;

public interface IAudioDeviceService
{
    IReadOnlyList<AudioOutputDevice> GetPlaybackDevices();
}
