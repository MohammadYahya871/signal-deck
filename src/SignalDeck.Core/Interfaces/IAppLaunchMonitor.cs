using SignalDeck.Core.Models;

namespace SignalDeck.Core.Interfaces;

public interface IAppLaunchMonitor
{
    event EventHandler<TriggerSignal>? Triggered;

    void Start(IEnumerable<string> processNames);

    void Stop();
}
