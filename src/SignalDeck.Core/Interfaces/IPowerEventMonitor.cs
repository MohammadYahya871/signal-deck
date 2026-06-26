using SignalDeck.Core.Models;

namespace SignalDeck.Core.Interfaces;

public interface IPowerEventMonitor
{
    event EventHandler<TriggerSignal>? Triggered;

    void Start();

    void Stop();
}
