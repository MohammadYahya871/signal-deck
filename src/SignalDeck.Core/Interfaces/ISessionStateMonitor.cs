using SignalDeck.Core.Models;

namespace SignalDeck.Core.Interfaces;

public interface ISessionStateMonitor
{
    event EventHandler<TriggerSignal>? Triggered;

    void Start();

    void Stop();
}
