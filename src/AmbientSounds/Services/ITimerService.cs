using System;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for a timer.
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// Triggered when the interval elapses.
        /// Interval is milliseconds.
        /// </summary>
        event EventHandler<int> IntervalElapsed;

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
}
