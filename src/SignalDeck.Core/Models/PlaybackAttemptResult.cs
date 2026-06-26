namespace SignalDeck.Core.Models;

public sealed class PlaybackAttemptResult
{
    public bool Succeeded { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? DeviceNameUsed { get; init; }

    public static PlaybackAttemptResult Success(string message, string? deviceNameUsed = null) =>
        new()
        {
            Succeeded = true,
            Message = message,
            DeviceNameUsed = deviceNameUsed
        };

    public static PlaybackAttemptResult Failure(string message) =>
        new()
        {
            Succeeded = false,
            Message = message
        };
}
