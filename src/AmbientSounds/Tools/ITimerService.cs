using System;

namespace AmbientSounds.Tools;

/// <summary>
/// Interface for a timer.
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// Triggered when the interval elapses.
    /// Interval is milliseconds.
    /// </summary>
    /// <remarks>
    /// This is not in the UI thread.
    /// </remarks>
    event EventHandler<TimeSpan>? IntervalElapsed;

    /// <summary>
    /// Interval in milliseconds.
    /// </summary>
    int Interval { get; set; }

    /// <summary>
    /// The time remaining.
    /// </summary>
    TimeSpan Remaining { get; set; }

    /// <summary>
    /// Start the timer.
    /// </summary>
    void Start();

    /// <summary>
    /// Stop the timer.
    /// </summary>
    void Stop();
}
