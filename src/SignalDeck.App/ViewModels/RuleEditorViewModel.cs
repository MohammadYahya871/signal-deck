using System.IO;
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
    private DeviceFallbackMode _deviceFallbackMode = DeviceFallbackMode.Skip;

    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                RaiseDerivedState();
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
                RaiseDerivedState();
            }
        }
    }

    public string AudioFilePath
    {
        get => _audioFilePath;
        set
        {
            if (SetProperty(ref _audioFilePath, value))
            {
                RaiseDerivedState();
            }
        }
    }

    public AudioOutputDevice? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetProperty(ref _selectedDevice, value))
            {
                RaiseDerivedState();
            }
        }
    }

    public int VolumePercent
    {
        get => _volumePercent;
        set
        {
            if (SetProperty(ref _volumePercent, value))
            {
                RaiseDerivedState();
            }
        }
    }

    public TriggerType TriggerType
    {
        get => _triggerType;
        set
        {
            if (SetProperty(ref _triggerType, value))
            {
                RaiseDerivedState();
                OnPropertyChanged(nameof(IsReturnAfterIdleTrigger));
                OnPropertyChanged(nameof(IsAppLaunchTrigger));
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
                RaiseDerivedState();
            }
        }
    }

    public int CooldownMinutes
    {
        get => _cooldownMinutes;
        set
        {
            if (SetProperty(ref _cooldownMinutes, value))
            {
                RaiseDerivedState();
            }
        }
    }

    public string AppProcessNamesText
    {
        get => _appProcessNamesText;
        set
        {
            if (SetProperty(ref _appProcessNamesText, value))
            {
                RaiseDerivedState();
            }
        }
    }

    public DeviceFallbackMode DeviceFallbackMode
    {
        get => _deviceFallbackMode;
        set
        {
            if (SetProperty(ref _deviceFallbackMode, value))
            {
                RaiseDerivedState();
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

    public string PlaybackSummary
    {
        get
        {
            var device = SelectedDevice?.Name ?? "No device selected";
            var file = string.IsNullOrWhiteSpace(AudioFilePath) ? "No audio selected" : Path.GetFileName(AudioFilePath);
            return $"{file} on {device} at {VolumePercent}%";
        }
    }

    public string StatusText
    {
        get
        {
            if (!IsEnabled)
            {
                return "Disabled";
            }

            var issues = GetValidationIssues();
            return issues.Count == 0 ? "Ready" : issues[0];
        }
    }

    public bool IsReady => GetValidationIssues().Count == 0;

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
                Volume = Math.Clamp(VolumePercent / 100f, 0f, 1f),
                DeviceFallbackMode = DeviceFallbackMode
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
            DeviceFallbackMode = rule.Playback.DeviceFallbackMode,
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

    public RuleEditorViewModel Clone(IReadOnlyCollection<AudioOutputDevice> devices)
    {
        var clone = FromModel(ToModel(), devices);
        clone.Id = Guid.NewGuid();
        clone.CreatedAt = DateTimeOffset.UtcNow;
        clone.Name = $"{Name} copy";
        return clone;
    }

    public IReadOnlyList<string> GetValidationIssues()
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            issues.Add("Rule name is required.");
        }

        if (string.IsNullOrWhiteSpace(AudioFilePath))
        {
            issues.Add("Audio file is required.");
        }
        else if (!File.Exists(AudioFilePath))
        {
            issues.Add("Audio file is missing.");
        }

        if (SelectedDevice is null)
        {
            issues.Add("Output device is required.");
        }

        if (TriggerType == TriggerType.ReturnAfterIdle && IdleThresholdMinutes < 1)
        {
            issues.Add("Idle threshold must be at least 1 minute.");
        }

        if (TriggerType == TriggerType.AppLaunch && ParseProcessNames(AppProcessNamesText).Count == 0)
        {
            issues.Add("Add at least one watched app.");
        }

        if (CooldownMinutes < 0)
        {
            issues.Add("Cooldown cannot be negative.");
        }

        return issues;
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

    private void RaiseDerivedState()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(TriggerSummary));
        OnPropertyChanged(nameof(PlaybackSummary));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(IsReady));
    }
}
