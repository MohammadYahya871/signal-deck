namespace SignalDeck.Core.Models;

public sealed class ActivityLogEntry
{
    public required DateTimeOffset OccurredAt { get; init; }

    public required string RuleName { get; init; }

    public required string Message { get; init; }

    public required bool IsSuccess { get; init; }
}
