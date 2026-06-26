namespace SignalDeck.Core.Models;

public sealed class AppSettings
{
    public bool LaunchAtSignIn { get; set; } = true;

    public List<SignalRule> Rules { get; set; } = [];

    public SignalRule? PrimaryRule { get; set; }
}
