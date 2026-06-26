using SignalDeck.Core.Models;

namespace SignalDeck.Core.Interfaces;

public interface IIdleMonitor
{
    event EventHandler<TriggerSignal>? Triggered;

    void Start(IEnumerable<int> idleThresholdSeconds);

    void Stop();
}
