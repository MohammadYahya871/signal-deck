using SignalDeck.Core.Models;

namespace SignalDeck.App.ViewModels;

public sealed class RuleEditorViewModel : ObservableObject
{
    private string _name = "New rule";
    private bool _isEnabled = true;
    private string _audioFilePath = string.Empty;
    private AudioOutputDevice? _selectedDevice;
    private int _volumePercent = 45;
    private TriggerType _triggerType = TriggerType.ReturnAfterIdle;
    private int _idleThresholdMinutes = 5;
    private int _cooldownMinutes = 10;
    private string _appProcessNamesText = string.Empty;

    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (SetProperty(ref _isEnabled, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string AudioFilePath
    {
        get => _audioFilePath;
        set => SetProperty(ref _audioFilePath, value);
    }

    public AudioOutputDevice? SelectedDevice
    {
        get => _selectedDevice;
        set => SetProperty(ref _selectedDevice, value);
    }

    public int VolumePercent
    {
        get => _volumePercent;
        set => SetProperty(ref _volumePercent, value);
    }

    public TriggerType TriggerType
    {
        get => _triggerType;
        set
        {
            if (SetProperty(ref _triggerType, value))
            {
                OnPropertyChanged(nameof(IsReturnAfterIdleTrigger));
                OnPropertyChanged(nameof(IsAppLaunchTrigger));
                OnPropertyChanged(nameof(TriggerSummary));
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public int IdleThresholdMinutes
    {
        get => _idleThresholdMinutes;
        set
        {
            if (SetProperty(ref _idleThresholdMinutes, value))
            {
                OnPropertyChanged(nameof(TriggerSummary));
            }
        }
    }

    public int CooldownMinutes
    {
        get => _cooldownMinutes;
        set => SetProperty(ref _cooldownMinutes, value);
    }

    public string AppProcessNamesText
    {
        get => _appProcessNamesText;
        set
        {
            if (SetProperty(ref _appProcessNamesText, value))
            {
                OnPropertyChanged(nameof(TriggerSummary));
            }
        }
    }

    public bool IsReturnAfterIdleTrigger => TriggerType == TriggerType.ReturnAfterIdle;

    public bool IsAppLaunchTrigger => TriggerType == TriggerType.AppLaunch;

    public string TriggerSummary => TriggerType switch
    {
        TriggerType.ReturnAfterIdle => $"Return after {IdleThresholdMinutes} min idle",
        TriggerType.AppLaunch => $"On app launch{BuildAppLaunchSuffix()}",
        TriggerType.SignIn => "At sign-in",
        TriggerType.SessionLock => "On session lock",
        TriggerType.SessionUnlock => "On session unlock",
        TriggerType.ResumeFromSleep => "On resume from sleep",
        _ => TriggerType.ToString()
    };

    public string DisplayName => $"{(IsEnabled ? string.Empty : "[Off] ")}{(string.IsNullOrWhiteSpace(Name) ? "Untitled rule" : Name)}";

    public SignalRule ToModel()
    {
        return new SignalRule
        {
            Id = Id,
            Name = string.IsNullOrWhiteSpace(Name) ? "Untitled rule" : Name.Trim(),
            Enabled = IsEnabled,
            Playback = new PlaybackSettings
            {
                AudioFilePath = AudioFilePath.Trim(),
                OutputDeviceId = SelectedDevice?.Id ?? string.Empty,
                OutputDeviceNameSnapshot = SelectedDevice?.Name ?? string.Empty,
                Volume = Math.Clamp(VolumePercent / 100f, 0f, 1f)
            },
            Trigger = new TriggerSettings
            {
                Type = TriggerType,
                IdleThresholdSeconds = Math.Max(1, IdleThresholdMinutes) * 60,
                AppProcessNames = ParseProcessNames(AppProcessNamesText)
            },
            Cooldown = new CooldownPolicy
            {
                Enabled = CooldownMinutes > 0,
                Seconds = Math.Max(0, CooldownMinutes) * 60
            },
            CreatedAt = CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public static RuleEditorViewModel FromModel(SignalRule rule, IReadOnlyCollection<AudioOutputDevice> devices)
    {
        var editor = new RuleEditorViewModel
        {
            Id = rule.Id,
            CreatedAt = rule.CreatedAt,
            Name = rule.Name,
            IsEnabled = rule.Enabled,
            AudioFilePath = rule.Playback.AudioFilePath,
            VolumePercent = (int)Math.Round(rule.Playback.Volume * 100),
            TriggerType = rule.Trigger.Type,
            IdleThresholdMinutes = Math.Max(1, rule.Trigger.IdleThresholdSeconds / 60),
            CooldownMinutes = Math.Max(0, rule.Cooldown.Seconds / 60),
            AppProcessNamesText = string.Join(Environment.NewLine, rule.Trigger.AppProcessNames)
        };

        editor.SelectedDevice = devices.FirstOrDefault(device => device.Id == rule.Playback.OutputDeviceId)
            ?? devices.FirstOrDefault();

        return editor;
    }

    public void AlignSelectedDevice(IReadOnlyCollection<AudioOutputDevice> devices)
    {
        if (SelectedDevice is null)
        {
            SelectedDevice = devices.FirstOrDefault();
            return;
        }

        SelectedDevice = devices.FirstOrDefault(device => device.Id == SelectedDevice.Id)
            ?? devices.FirstOrDefault();
    }

    private static List<string> ParseProcessNames(string input)
    {
        return input
            .Split([',', '\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(processName => processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? processName
                : $"{processName}.exe")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string BuildAppLaunchSuffix()
    {
        var count = ParseProcessNames(AppProcessNamesText).Count;
        return count > 0 ? $" ({count} app{(count == 1 ? string.Empty : "s")})" : string.Empty;
    }
}
