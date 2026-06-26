using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;
using SignalDeck.Core.Services;

namespace SignalDeck.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly IAudioDeviceService _audioDeviceService;
    private readonly IAudioPlaybackService _audioPlaybackService;
    private readonly ISettingsStore _settingsStore;
    private readonly IStartupRegistrationService _startupRegistrationService;
    private readonly SignalDeckCoordinator _coordinator;
    private AppSettings _settings = new();
    private RuleEditorViewModel? _selectedRule;
    private bool _launchAtSignIn = true;
    private string _statusMessage = "Configure one or more playback rules.";

    public MainWindowViewModel(
        IAudioDeviceService audioDeviceService,
        IAudioPlaybackService audioPlaybackService,
        ISettingsStore settingsStore,
        IStartupRegistrationService startupRegistrationService,
        SignalDeckCoordinator coordinator)
    {
        _audioDeviceService = audioDeviceService;
        _audioPlaybackService = audioPlaybackService;
        _settingsStore = settingsStore;
        _startupRegistrationService = startupRegistrationService;
        _coordinator = coordinator;

        TriggerTypes =
        [
            new TriggerTypeOption(TriggerType.ReturnAfterIdle, "Return After Idle"),
            new TriggerTypeOption(TriggerType.SignIn, "At Sign-In"),
            new TriggerTypeOption(TriggerType.AppLaunch, "When Selected App Starts"),
            new TriggerTypeOption(TriggerType.SessionLock, "When Session Locks"),
            new TriggerTypeOption(TriggerType.SessionUnlock, "When Session Unlocks"),
            new TriggerTypeOption(TriggerType.ResumeFromSleep, "When Windows Resumes")
        ];
        AddRuleCommand = new RelayCommand(AddRule);
        RemoveRuleCommand = new RelayCommand(RemoveSelectedRule);
        BrowseAudioCommand = new RelayCommand(BrowseAudioFileForSelectedRule);
        PreviewAudioCommand = new RelayCommand(async () => await PreviewSelectedRuleAsync());
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        RefreshDevicesCommand = new RelayCommand(LoadDevices);
    }

    public ObservableCollection<AudioOutputDevice> Devices { get; } = [];

    public ObservableCollection<RuleEditorViewModel> Rules { get; } = [];

    public IReadOnlyList<TriggerTypeOption> TriggerTypes { get; }

    public ICommand AddRuleCommand { get; }

    public ICommand RemoveRuleCommand { get; }

    public ICommand BrowseAudioCommand { get; }

    public ICommand PreviewAudioCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand RefreshDevicesCommand { get; }

    public RuleEditorViewModel? SelectedRule
    {
        get => _selectedRule;
        set => SetProperty(ref _selectedRule, value);
    }

    public bool LaunchAtSignIn
    {
        get => _launchAtSignIn;
        set => SetProperty(ref _launchAtSignIn, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public Task InitializeAsync(AppSettings settings)
    {
        _settings = NormalizeSettings(settings);
        LoadDevices();
        Rules.Clear();

        foreach (var rule in _settings.Rules)
        {
            Rules.Add(RuleEditorViewModel.FromModel(rule, Devices));
        }

        LaunchAtSignIn = _settings.LaunchAtSignIn;
        SelectedRule = Rules.FirstOrDefault();
        return Task.CompletedTask;
    }

    private void LoadDevices()
    {
        Devices.Clear();

        foreach (var device in _audioDeviceService.GetPlaybackDevices())
        {
            Devices.Add(device);
        }

        foreach (var rule in Rules)
        {
            rule.AlignSelectedDevice(Devices);
        }

        SelectedRule?.AlignSelectedDevice(Devices);
    }

    private void AddRule()
    {
        var newRule = new RuleEditorViewModel
        {
            Name = $"Rule {Rules.Count + 1}",
            SelectedDevice = Devices.FirstOrDefault()
        };

        Rules.Add(newRule);
        SelectedRule = newRule;
        StatusMessage = "New rule added.";
    }

    private void RemoveSelectedRule()
    {
        if (SelectedRule is null)
        {
            StatusMessage = "Select a rule to remove.";
            return;
        }

        var ruleToRemove = SelectedRule;
        Rules.Remove(ruleToRemove);

        if (Rules.Count == 0)
        {
            AddRule();
        }
        else
        {
            SelectedRule = Rules.FirstOrDefault();
        }

        StatusMessage = $"Removed rule \"{ruleToRemove.Name}\".";
    }

    private void BrowseAudioFileForSelectedRule()
    {
        if (SelectedRule is null)
        {
            StatusMessage = "Select a rule before browsing for audio.";
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Audio Files|*.mp3;*.wav;*.flac;*.aac;*.m4a;*.wma|All Files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedRule.AudioFilePath = dialog.FileName;
        }
    }

    private async Task SaveAsync()
    {
        if (Rules.Any(rule => rule.SelectedDevice is null))
        {
            StatusMessage = "Every rule needs an output device before saving.";
            return;
        }

        _settings = new AppSettings
        {
            LaunchAtSignIn = LaunchAtSignIn,
            Rules = Rules.Select(rule => rule.ToModel()).ToList()
        };

        await _settingsStore.SaveAsync(_settings);
        _startupRegistrationService.SetEnabled(LaunchAtSignIn);
        _coordinator.ApplySettings(_settings);
        StatusMessage = "SignalDeck settings saved.";
    }

    private async Task PreviewSelectedRuleAsync()
    {
        if (SelectedRule is null)
        {
            StatusMessage = "Select a rule to preview.";
            return;
        }

        if (SelectedRule.SelectedDevice is null)
        {
            StatusMessage = "Choose an output device before previewing.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedRule.AudioFilePath))
        {
            StatusMessage = "Choose an audio file before previewing.";
            return;
        }

        if (!File.Exists(SelectedRule.AudioFilePath))
        {
            StatusMessage = "The selected audio file could not be found.";
            return;
        }

        try
        {
            StatusMessage = "Playing preview...";
            await _audioPlaybackService.PlayAsync(SelectedRule.ToModel().Playback);
            StatusMessage = "Preview finished.";
        }
        catch
        {
            StatusMessage = "Preview failed. Check the audio file and output device.";
        }
    }

    private static AppSettings NormalizeSettings(AppSettings settings)
    {
        if (settings.Rules.Count == 0 && settings.PrimaryRule is not null)
        {
            settings.Rules.Add(settings.PrimaryRule);
        }

        if (settings.Rules.Count == 0)
        {
            settings.Rules.Add(new SignalRule
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
            });
        }

        return settings;
    }
}
