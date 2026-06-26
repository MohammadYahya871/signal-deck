using NAudio.CoreAudioApi;
using SignalDeck.Core.Interfaces;

namespace SignalDeck.Infrastructure.Idle;

public sealed class AudioSessionPlaybackMonitor : IPlaybackActivityMonitor
{
    public bool IsPlaybackActive()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in devices)
            {
                using (device)
                {
                    var sessions = device.AudioSessionManager.Sessions;
                    for (var i = 0; i < sessions.Count; i++)
                    {
                        using var session = sessions[i];
                        if (string.Equals(session.State.ToString(), "Active", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch
        {
            // If system media state cannot be read, fall back to normal idle behavior.
        }

        return false;
    }
}
