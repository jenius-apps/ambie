using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services.Xamarin
{
    public class Timer : ITimerService
    {
        /// <inheritdoc/>
        public int Interval { get; set; }

        /// <inheritdoc/>
        public TimeSpan Remaining { get; set; }

        /// <inheritdoc/>

        public event EventHandler<TimeSpan> IntervalElapsed;

        /// <inheritdoc/>
        public void Start()
        {
        }

        /// <inheritdoc/>
        public void Stop()
        {
        }
    }
}
