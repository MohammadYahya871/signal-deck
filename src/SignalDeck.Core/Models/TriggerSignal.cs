namespace SignalDeck.Core.Models;

public sealed class TriggerSignal
{
    public required TriggerType Type { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public TimeSpan? IdleDuration { get; init; }

    public string? ProcessName { get; init; }
}
