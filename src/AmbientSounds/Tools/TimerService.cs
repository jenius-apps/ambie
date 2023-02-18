using AmbientSounds.Services;
using System;
using System.Timers;

namespace AmbientSounds.Tools;

/// <summary>
/// Class for handling a timer countdown.
/// </summary>
public class TimerService : ITimerService
{
    /// <inheritdoc/>
    public event EventHandler<TimeSpan>? IntervalElapsed;

    private readonly Timer _timer;

    public TimerService()
    {
        _timer = new Timer();
        _timer.Elapsed += TimerIntervalElapsed;
    }

    /// <inheritdoc/>
    public int Interval
    {
        get => (int)_timer.Interval;
        set => _timer.Interval = value;
    }

    /// <inheritdoc/>
    public TimeSpan Remaining { get; set; }

    /// <inheritdoc/>
    public void Start()
    {
        _timer.Start();
    }

    /// <inheritdoc/>
    public void Stop()
    {
        _timer.Stop();
    }

    private void TimerIntervalElapsed(object sender, object e)
    {
        Remaining -= TimeSpan.FromMilliseconds(Interval);
        IntervalElapsed?.Invoke(sender, Remaining);
    }
}