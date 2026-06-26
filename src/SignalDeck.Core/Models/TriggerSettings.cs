namespace SignalDeck.Core.Models;

public sealed class TriggerSettings
{
    public TriggerType Type { get; set; } = TriggerType.ReturnAfterIdle;

    public int IdleThresholdSeconds { get; set; } = 300;

    public List<string> AppProcessNames { get; set; } = [];
}
