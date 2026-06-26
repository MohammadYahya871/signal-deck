using Microsoft.Win32;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;

namespace SignalDeck.Infrastructure.Session;

public sealed class SessionStateMonitor : ISessionStateMonitor
{
    private readonly TimeProvider _timeProvider;
    private bool _started;

    public SessionStateMonitor(TimeProvider timeProvider)
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

        SystemEvents.SessionSwitch += OnSessionSwitch;
        _started = true;
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }

        SystemEvents.SessionSwitch -= OnSessionSwitch;
        _started = false;
    }

    private void OnSessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        TriggerType? triggerType = e.Reason switch
        {
            SessionSwitchReason.SessionLock => TriggerType.SessionLock,
            SessionSwitchReason.SessionUnlock => TriggerType.SessionUnlock,
            _ => null
        };

        if (triggerType is null)
        {
            return;
        }

        Triggered?.Invoke(this, new TriggerSignal
        {
            Type = triggerType.Value,
            OccurredAt = _timeProvider.GetUtcNow()
        });
    }
}
