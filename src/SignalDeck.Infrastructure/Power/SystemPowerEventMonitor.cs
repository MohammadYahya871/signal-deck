using Microsoft.Win32;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;

namespace SignalDeck.Infrastructure.Power;

public sealed class SystemPowerEventMonitor : IPowerEventMonitor
{
    private readonly TimeProvider _timeProvider;
    private bool _started;

    public SystemPowerEventMonitor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public event EventHandler<TriggerSignal>? Triggered;

    public void Start()
    {
        if (_started)
        {
            return;
        }

        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        _started = true;
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }

        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        _started = false;
    }

    private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume)
        {
            return;
        }

        Triggered?.Invoke(this, new TriggerSignal
        {
            Type = TriggerType.ResumeFromSleep,
            OccurredAt = _timeProvider.GetUtcNow()
        });
    }
}
