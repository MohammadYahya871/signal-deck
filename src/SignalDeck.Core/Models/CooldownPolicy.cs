namespace SignalDeck.Core.Models;

public sealed class CooldownPolicy
{
    public bool Enabled { get; set; } = true;

    public int Seconds { get; set; } = 600;
}
