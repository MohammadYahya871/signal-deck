namespace SignalDeck.Core.Models;

public sealed class SignalRule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "New rule";

    public bool Enabled { get; set; } = true;

    public TriggerSettings Trigger { get; set; } = new();

    public PlaybackSettings Playback { get; set; } = new();

    public CooldownPolicy Cooldown { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
