using System;
using System.Timers;
using Windows.System;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for handling a timer countdown.
    /// </summary>
    public class TimerService : ITimerService
    {
        /// <inheritdoc/>
        public event EventHandler<int>? IntervalElapsed;

        private readonly Timer _timer;
        private readonly DispatcherQueue _dispatcherQueue;

        public TimerService()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
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
            _dispatcherQueue.TryEnqueue(() => IntervalElapsed?.Invoke(sender, Interval));
        }
    }
}
