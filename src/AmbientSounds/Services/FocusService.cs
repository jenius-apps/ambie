using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services
{
    public class FocusService : IFocusService
    {
        private readonly ITimerService _timerService;
        private readonly Queue<FocusSession> _sessionQueue = new();

        public event EventHandler<FocusSession>? TimeUpdated;

        public FocusService(ITimerService timerService)
        {
            Guard.IsNotNull(timerService, nameof(timerService));
            _timerService = timerService;

            _timerService.Interval = 1000;
            _timerService.IntervalElapsed += OnIntervalElapsed;
        }

        public FocusSession CurrentSession { get; private set; } = new FocusSession(SessionType.None, TimeSpan.Zero, 0, 0);

        public FocusState CurrentState { get; private set; } = FocusState.None;

        public void StartTimer(int focusLength, int restLength, int repetitions)
        {
            if (focusLength == 0 || restLength == 0)
            {
                return;
            }

            _timerService.Stop();
            _sessionQueue.Clear();

            int queueSize = (repetitions + 1) * 2;
            int queuePosition = 0;

            while (repetitions >= 0)
            {
                _sessionQueue.Enqueue(new FocusSession(
                    SessionType.Focus,
                    TimeSpan.FromMinutes(focusLength),
                    queuePosition++,
                    queueSize));

                _sessionQueue.Enqueue(new FocusSession(
                    SessionType.Rest,
                    TimeSpan.FromMinutes(restLength),
                    queuePosition++,
                    queueSize));

                repetitions -= 1;
            }

            CurrentSession = _sessionQueue.Dequeue();
            _timerService.Remaining = CurrentSession.Remaining;
            _timerService.Start();
            CurrentState = FocusState.Active;
        }

        public void PauseTimer()
        {
            _timerService.Stop();
            CurrentState = FocusState.Paused;
        }

        public void StopTimer()
        {
            _timerService.Stop();
            CurrentSession = new FocusSession(SessionType.None, TimeSpan.Zero, 0, 0);
            TimeUpdated?.Invoke(this, CurrentSession);
            CurrentState = FocusState.None;
        }

        public void ResumeTimer()
        {
            if (_timerService.Remaining > TimeSpan.Zero)
            {
                _timerService.Start();
                CurrentState = FocusState.Active;
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

        public int GetRepetitionsRemaining(FocusSession session)
        {
            if (session is null || session.QueueSize <= session.QueuePosition)
            {
                return 0;
            }

            return (int)(Math.Ceiling((session.QueueSize - session.QueuePosition) / 2d) - 1);
        }

        private void OnIntervalElapsed(object sender, TimeSpan remaining)
        {
            CurrentSession.Remaining = remaining;
            TimeUpdated?.Invoke(this, CurrentSession);

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

    public enum FocusState
    {
        None,
        Active,
        Paused
    }

    public enum SessionType
    {
        None,
        Focus,
        Rest
    }

    public class FocusSession
    {
        public FocusSession(SessionType sessionType, TimeSpan remaining, int queuePosition, int queueSize)
        {
            SessionType = sessionType;
            Remaining = remaining;
            QueuePosition = queuePosition;
            QueueSize = queueSize;
        }

        public SessionType SessionType { get; }

        public TimeSpan Remaining { get; set; }

        public int QueueSize { get; }

        public int QueuePosition { get; }
    }
}
