using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services
{
    public class FocusService : IFocusService
    {
        private readonly ITimerService _timerService;

        public event EventHandler<FocusSession>? TimeUpdated;

        private readonly Queue<FocusSession> _sessionQueue = new();

        public FocusService(ITimerService timerService)
        {
            Guard.IsNotNull(timerService, nameof(timerService));
            _timerService = timerService;

            _timerService.Interval = 1000;
            _timerService.IntervalElapsed += OnIntervalElapsed;
        }

        public FocusSession CurrentSession { get; private set; } = new FocusSession(SessionType.None, TimeSpan.Zero);

        public void StartTimer(int focusLength, int restLength, int repetitions)
        {
            _timerService.Stop();
            _sessionQueue.Clear();

            while (repetitions >= 0)
            {
                _sessionQueue.Enqueue(new FocusSession(SessionType.Focus, TimeSpan.FromMinutes(focusLength)));
                _sessionQueue.Enqueue(new FocusSession(SessionType.Rest, TimeSpan.FromMinutes(restLength)));
                repetitions -= 1;
            }

            CurrentSession = _sessionQueue.Dequeue();
            _timerService.Remaining = CurrentSession.Remaining;
            _timerService.Start();
        }

        public void PauseTimer()
        {
            _timerService.Stop();
        }

        public void StopTimer()
        {
            _timerService.Stop();
            CurrentSession = new FocusSession(SessionType.None, TimeSpan.Zero);
            TimeUpdated?.Invoke(this, CurrentSession);
        }

        public void ResumeTimer()
        {
            if (_timerService.Remaining > TimeSpan.Zero)
            {
                _timerService.Start();
            }
        }

        public TimeSpan GetTotalTime(int focusLength, int restLength, int repetitions)
        {
            if (focusLength < 0 ||
                restLength < 0 ||
                repetitions < 0)
            {
                return TimeSpan.Zero;
            }

            repetitions += 1;
            return TimeSpan.FromMinutes((focusLength + restLength) * repetitions);
        }

        private void OnIntervalElapsed(object sender, TimeSpan remaining)
        {
            TimeUpdated?.Invoke(this, new FocusSession(CurrentSession.SessionType, remaining));

            if (remaining == TimeSpan.Zero)
            {
                _timerService.Stop();

                if (_sessionQueue.Count > 0)
                {
                    CurrentSession = _sessionQueue.Dequeue();
                    _timerService.Remaining = CurrentSession.Remaining;
                    _timerService.Start();
                }
            }
        }
    }

    public enum SessionType
    {
        None,
        Focus,
        Rest
    }

    public record FocusSession(SessionType SessionType, TimeSpan Remaining);
}
