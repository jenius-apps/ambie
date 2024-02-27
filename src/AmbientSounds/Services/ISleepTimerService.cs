using System;

namespace AmbientSounds.Services
{
    public interface ISleepTimerService
    {
        /// <summary>
        /// Time left in the sleep timer in the form of a
        /// decrementing percentage. E.g. 100 > 99 > 98...
        /// </summary>
        double PercentLeft { get; }

        /// <summary>
        /// Time left in the sleep timer in the form of a TimeSpan.
        /// </summary>
        TimeSpan TimeLeft { get; }

        /// <summary>
        /// The current <see cref="SleepTimerState"/>.
        /// </summary>
        SleepTimerState State { get; }

        /// <summary>
        /// Raised when the timer is running and its interval elapses.
        /// </summary>
        event EventHandler<TimeSpan>? TimerIntervalElapsed;

        /// <summary>
        /// Raised when the state of the timer changes.
        /// </summary>
        event EventHandler<SleepTimerState>? StateChanged;

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        void PauseTimer();

        /// <summary>
        /// Resumes the timer. If <see cref="StartTimer(int)"/>
        /// had not been run yet, then this is a no-op.
        /// </summary>
        void ResumeTimer();

        /// <summary>
        /// Starts the timer using the given number of minutes as the
        /// countdown time.
        /// </summary>
        /// <param name="minutes">The length of the timer in minutes.</param>
        void StartTimer(int minutes);

        /// <summary>
        /// Stops the timer and resets the countdown to 0.
        /// </summary>
        void StopTimer();
    }
}