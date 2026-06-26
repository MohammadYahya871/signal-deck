namespace SignalDeck.Core.Models;

public sealed class PlaybackSettings
{
    public string AudioFilePath { get; set; } = string.Empty;

    public string OutputDeviceId { get; set; } = string.Empty;

    public string OutputDeviceNameSnapshot { get; set; } = string.Empty;

    public float Volume { get; set; } = 0.45f;
}
