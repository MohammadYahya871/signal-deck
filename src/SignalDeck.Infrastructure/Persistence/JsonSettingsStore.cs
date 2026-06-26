using System.Text.Json;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;

namespace SignalDeck.Infrastructure.Persistence;

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;

    public JsonSettingsStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(appData, "SignalDeck");
        Directory.CreateDirectory(directory);
        _settingsPath = Path.Combine(directory, "settings.json");
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsPath))
        {
            return CreateDefaultSettings();
        }

        await using var stream = File.OpenRead(_settingsPath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, SerializerOptions, cancellationToken);
        return Normalize(settings);
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions, cancellationToken);
    }

    private static AppSettings Normalize(AppSettings? settings)
    {
        settings ??= CreateDefaultSettings();

        if (settings.Rules.Count == 0 && settings.PrimaryRule is not null)
        {
            settings.Rules.Add(settings.PrimaryRule);
        }

        if (settings.Rules.Count == 0)
        {
            settings.Rules.Add(CreateDefaultRule());
        }

        return settings;
    }

    private static AppSettings CreateDefaultSettings()
    {
        return new AppSettings
        {
            Rules = [CreateDefaultRule()]
        };
    }

    private static SignalRule CreateDefaultRule()
    {
        return new SignalRule
        {
            Name = "Return greeting",
            Trigger = new TriggerSettings
            {
                Type = TriggerType.ReturnAfterIdle,
                IdleThresholdSeconds = 300
            },
            Cooldown = new CooldownPolicy
            {
                Enabled = true,
                Seconds = 600
            },
            Playback = new PlaybackSettings
            {
                Volume = 0.45f
            }
        };
    }
}
