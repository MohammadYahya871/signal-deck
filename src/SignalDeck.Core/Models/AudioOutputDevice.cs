namespace SignalDeck.Core.Models;

public sealed class AudioOutputDevice
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public override string ToString() => Name;
}
