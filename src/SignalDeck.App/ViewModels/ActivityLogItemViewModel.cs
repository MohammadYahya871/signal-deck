namespace SignalDeck.App.ViewModels;

public sealed class ActivityLogItemViewModel
{
    public required string Timestamp { get; init; }

    public required string RuleName { get; init; }

    public required string Message { get; init; }

    public required bool IsSuccess { get; init; }
}
