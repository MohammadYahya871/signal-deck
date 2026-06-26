using System.Diagnostics;
using System.Timers;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;

namespace SignalDeck.Infrastructure.AppLaunch;

public sealed class PollingAppLaunchMonitor : IAppLaunchMonitor, IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly TimeProvider _timeProvider;
    private HashSet<string> _watchedProcesses = [];
    private HashSet<string> _knownRunningProcesses = [];

    public PollingAppLaunchMonitor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _timer = new System.Timers.Timer(2000);
        _timer.Elapsed += OnTimerElapsed;
    }

    public event EventHandler<TriggerSignal>? Triggered;

    public void Start(IEnumerable<string> processNames)
    {
        _watchedProcesses = processNames
            .Select(NormalizeProcessName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _knownRunningProcesses = CaptureWatchedProcesses();

        if (_watchedProcesses.Count > 0)
        {
            _timer.Start();
        }
    }

    public void Stop()
    {
        _timer.Stop();
        _knownRunningProcesses.Clear();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var current = CaptureWatchedProcesses();
        var launched = current.Except(_knownRunningProcesses, StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var processName in launched)
        {
            Triggered?.Invoke(this, new TriggerSignal
            {
                Type = TriggerType.AppLaunch,
                ProcessName = processName,
                OccurredAt = _timeProvider.GetUtcNow()
            });
        }

        _knownRunningProcesses = current;
    }

    private HashSet<string> CaptureWatchedProcesses()
    {
        var running = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var processName = NormalizeProcessName(process.ProcessName);
                if (_watchedProcesses.Contains(processName))
                {
                    running.Add(processName);
                }
            }
            catch
            {
                // Ignore transient process inspection failures.
            }
            finally
            {
                process.Dispose();
            }
        }

        return running;
    }

    private static string NormalizeProcessName(string? processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return string.Empty;
        }

        var normalized = processName.Trim();
        if (normalized.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^4];
        }

        return normalized.ToLowerInvariant();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
