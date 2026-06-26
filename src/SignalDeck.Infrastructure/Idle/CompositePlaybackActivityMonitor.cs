using SignalDeck.Core.Interfaces;

namespace SignalDeck.Infrastructure.Idle;

public sealed class CompositePlaybackActivityMonitor : IPlaybackActivityMonitor
{
    private readonly IReadOnlyList<IPlaybackActivityMonitor> _monitors;

    public CompositePlaybackActivityMonitor(IEnumerable<IPlaybackActivityMonitor> monitors)
    {
        _monitors = monitors.ToList();
    }

    public bool IsPlaybackActive()
    {
        foreach (var monitor in _monitors)
        {
            if (monitor.IsPlaybackActive())
            {
                return true;
            }
        }

        return false;
    }
}
