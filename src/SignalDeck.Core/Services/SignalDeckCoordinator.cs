using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;

namespace SignalDeck.Core.Services;

public sealed class SignalDeckCoordinator : IDisposable
{
    private readonly IIdleMonitor _idleMonitor;
    private readonly IAppLaunchMonitor _appLaunchMonitor;
    private readonly ISessionStateMonitor _sessionStateMonitor;
    private readonly IPowerEventMonitor _powerEventMonitor;
    private readonly IAudioPlaybackService _audioPlaybackService;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _playbackLock = new(1, 1);
    private AppSettings _settings = new();
    private readonly Dictionary<Guid, DateTimeOffset> _lastPlaybackStartedAt = [];
    private bool _started;

    public SignalDeckCoordinator(
        IIdleMonitor idleMonitor,
        IAppLaunchMonitor appLaunchMonitor,
        ISessionStateMonitor sessionStateMonitor,
        IPowerEventMonitor powerEventMonitor,
        IAudioPlaybackService audioPlaybackService,
        TimeProvider timeProvider)
    {
        _idleMonitor = idleMonitor;
        _appLaunchMonitor = appLaunchMonitor;
        _sessionStateMonitor = sessionStateMonitor;
        _powerEventMonitor = powerEventMonitor;
        _audioPlaybackService = audioPlaybackService;
        _timeProvider = timeProvider;
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = NormalizeSettings(settings);
        if (_started)
        {
            ConfigureMonitors();
        }
    }

    public void Start()
    {
        _idleMonitor.Triggered += OnTriggerReceived;
        _appLaunchMonitor.Triggered += OnTriggerReceived;
        _sessionStateMonitor.Triggered += OnTriggerReceived;
        _powerEventMonitor.Triggered += OnTriggerReceived;
        _started = true;
        ConfigureMonitors();
        _ = FireSignInTriggersAsync();
    }

    public void Stop()
    {
        _idleMonitor.Triggered -= OnTriggerReceived;
        _appLaunchMonitor.Triggered -= OnTriggerReceived;
        _sessionStateMonitor.Triggered -= OnTriggerReceived;
        _powerEventMonitor.Triggered -= OnTriggerReceived;
        _idleMonitor.Stop();
        _appLaunchMonitor.Stop();
        _sessionStateMonitor.Stop();
        _powerEventMonitor.Stop();
        _started = false;
    }

    private static AppSettings NormalizeSettings(AppSettings settings)
    {
        if (settings.Rules.Count == 0 && settings.PrimaryRule is not null)
        {
            settings.Rules.Add(settings.PrimaryRule);
        }

        if (settings.Rules.Count == 0)
        {
            settings.Rules.Add(new SignalRule());
        }

        return settings;
    }

    private void ConfigureMonitors()
    {
        _idleMonitor.Stop();
        _appLaunchMonitor.Stop();
        _sessionStateMonitor.Stop();
        _powerEventMonitor.Stop();

        var enabledRules = _settings.Rules.Where(rule => rule.Enabled).ToList();

        var idleThresholds = enabledRules
            .Where(rule => rule.Trigger.Type == TriggerType.ReturnAfterIdle)
            .Select(rule => rule.Trigger.IdleThresholdSeconds)
            .Where(seconds => seconds > 0)
            .Distinct()
            .ToList();

        if (idleThresholds.Count > 0)
        {
            _idleMonitor.Start(idleThresholds);
        }

        var watchedProcesses = enabledRules
            .Where(rule => rule.Trigger.Type == TriggerType.AppLaunch)
            .SelectMany(rule => rule.Trigger.AppProcessNames)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (watchedProcesses.Count > 0)
        {
            _appLaunchMonitor.Start(watchedProcesses);
        }

        if (enabledRules.Any(rule => rule.Trigger.Type is TriggerType.SessionLock or TriggerType.SessionUnlock))
        {
            _sessionStateMonitor.Start();
        }

        if (enabledRules.Any(rule => rule.Trigger.Type == TriggerType.ResumeFromSleep))
        {
            _powerEventMonitor.Start();
        }
    }

    private async void OnTriggerReceived(object? sender, TriggerSignal signal)
    {
        try
        {
            var matchingRules = _settings.Rules
                .Where(rule => rule.Enabled)
                .Where(rule => rule.Trigger.Type == signal.Type)
                .Where(rule => Matches(rule, signal))
                .ToList();

            foreach (var rule in matchingRules)
            {
                await TryPlayRuleAsync(rule);
            }
        }
        catch
        {
            // Phase 2 still keeps runtime failures isolated so monitoring can continue.
        }
    }

    private async Task FireSignInTriggersAsync()
    {
        var rules = _settings.Rules
            .Where(rule => rule.Enabled && rule.Trigger.Type == TriggerType.SignIn)
            .ToList();

        foreach (var rule in rules)
        {
            await TryPlayRuleAsync(rule);
        }
    }

    private static bool Matches(SignalRule rule, TriggerSignal signal)
    {
        return signal.Type switch
        {
            TriggerType.ReturnAfterIdle => signal.IdleDuration is not null &&
                                           signal.IdleDuration.Value >= TimeSpan.FromSeconds(rule.Trigger.IdleThresholdSeconds),
            TriggerType.AppLaunch => !string.IsNullOrWhiteSpace(signal.ProcessName) &&
                                     rule.Trigger.AppProcessNames.Any(name =>
                                         string.Equals(NormalizeProcessName(name), NormalizeProcessName(signal.ProcessName), StringComparison.OrdinalIgnoreCase)),
            _ => true
        };
    }

    private async Task TryPlayRuleAsync(SignalRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Playback.AudioFilePath))
        {
            return;
        }

        await _playbackLock.WaitAsync();
        try
        {
            if (rule.Cooldown.Enabled &&
                _lastPlaybackStartedAt.TryGetValue(rule.Id, out var lastPlaybackStartedAt))
            {
                var elapsed = _timeProvider.GetUtcNow() - lastPlaybackStartedAt;
                if (elapsed < TimeSpan.FromSeconds(rule.Cooldown.Seconds))
                {
                    return;
                }
            }

            _lastPlaybackStartedAt[rule.Id] = _timeProvider.GetUtcNow();
            await _audioPlaybackService.PlayAsync(rule.Playback);
        }
        finally
        {
            _playbackLock.Release();
        }
    }

    private static string NormalizeProcessName(string? processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return string.Empty;
        }

        var normalized = processName.Trim();
        if (normalized.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^4];
        }

        return normalized.ToLowerInvariant();
    }

    public void Dispose()
    {
        Stop();
        _playbackLock.Dispose();
    }
}
