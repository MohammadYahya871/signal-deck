using NAudio.CoreAudioApi;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;

namespace SignalDeck.Infrastructure.Audio;

public sealed class NaudioDeviceService : IAudioDeviceService
{
    public IReadOnlyList<AudioOutputDevice> GetPlaybackDevices()
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        return devices
            .Select(device => new AudioOutputDevice
            {
                Id = device.ID,
                Name = device.FriendlyName
            })
            .ToList();
    }
}
