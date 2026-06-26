using System.Runtime.InteropServices;
using System.Timers;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Models;

namespace SignalDeck.Infrastructure.Idle;

public sealed class Win32IdleMonitor : IIdleMonitor, IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly TimeProvider _timeProvider;
    private readonly IPlaybackActivityMonitor _playbackActivityMonitor;
    private bool _wasIdle;
    private TimeSpan _lastIdleDuration = TimeSpan.Zero;
    private TimeSpan _effectiveIdleDuration = TimeSpan.Zero;
    private long _lastSampleTick;
    private uint _lastInputTick;
    private int _minimumIdleThresholdSeconds;

    public Win32IdleMonitor(TimeProvider timeProvider, IPlaybackActivityMonitor playbackActivityMonitor)
    {
        _timeProvider = timeProvider;
        _playbackActivityMonitor = playbackActivityMonitor;
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
    }

    public event EventHandler<TriggerSignal>? Triggered;

    public void Start(IEnumerable<int> idleThresholdSeconds)
    {
        var thresholds = idleThresholdSeconds
            .Where(seconds => seconds > 0)
            .Distinct()
            .ToList();

        if (thresholds.Count == 0)
        {
            Stop();
            return;
        }

        _minimumIdleThresholdSeconds = thresholds.Min();
        _lastSampleTick = Environment.TickCount64;
        _lastInputTick = GetLastInputTick();
        _effectiveIdleDuration = _playbackActivityMonitor.IsPlaybackActive()
            ? TimeSpan.Zero
            : GetRawIdleTime();
        _wasIdle = _effectiveIdleDuration >= TimeSpan.FromSeconds(_minimumIdleThresholdSeconds);
        _lastIdleDuration = _wasIdle ? _effectiveIdleDuration : TimeSpan.Zero;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _wasIdle = false;
        _effectiveIdleDuration = TimeSpan.Zero;
        _lastIdleDuration = TimeSpan.Zero;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var currentTick = Environment.TickCount64;
        var currentInputTick = GetLastInputTick();
        var elapsedTicks = Math.Max(0, currentTick - _lastSampleTick);

        if (currentInputTick != _lastInputTick)
        {
            if (_wasIdle)
            {
                Triggered?.Invoke(this, new TriggerSignal
                {
                    Type = TriggerType.ReturnAfterIdle,
                    IdleDuration = _lastIdleDuration,
                    OccurredAt = _timeProvider.GetUtcNow()
                });
            }

            _effectiveIdleDuration = TimeSpan.Zero;
            _lastIdleDuration = TimeSpan.Zero;
            _wasIdle = false;
            _lastInputTick = currentInputTick;
            _lastSampleTick = currentTick;
            return;
        }

        if (!_playbackActivityMonitor.IsPlaybackActive())
        {
            _effectiveIdleDuration += TimeSpan.FromMilliseconds(elapsedTicks);
        }

        var isIdle = _effectiveIdleDuration >= TimeSpan.FromSeconds(_minimumIdleThresholdSeconds);

        if (isIdle)
        {
            _lastIdleDuration = _effectiveIdleDuration;
        }

        _wasIdle = isIdle;
        _lastSampleTick = currentTick;
    }

    private static TimeSpan GetRawIdleTime()
    {
        var info = new LASTINPUTINFO
        {
            cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
        };

        if (!GetLastInputInfo(ref info))
        {
            return TimeSpan.Zero;
        }

        var idleMilliseconds = Environment.TickCount64 - info.dwTime;
        return TimeSpan.FromMilliseconds(Math.Max(0, idleMilliseconds));
    }

    private static uint GetLastInputTick()
    {
        var info = new LASTINPUTINFO
        {
            cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
        };

        if (!GetLastInputInfo(ref info))
        {
            return 0;
        }

        return info.dwTime;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }
}
