using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using System;
using System.Collections.Generic;

namespace AmbientSounds.Services;

public class FocusService : IFocusService
{
    private readonly ITimerService _timerService;
    private readonly IFocusToastService _focusToastService;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly ITelemetry _telemetry;
    private readonly IFocusHistoryService _focusHistoryService;
    private readonly IUserSettings _userSettings;
    private readonly Queue<FocusSession> _sessionQueue = new();
    private FocusState _focusState = FocusState.None;
    private double _previousGlobalVolume;
    private bool _skipSegmentRequested;

    public event EventHandler<FocusSession>? TimeUpdated;
    public event EventHandler<FocusState>? FocusStateChanged;

    public FocusService(
        ITimerService timerService,
        IFocusToastService focusToastService,
        IMixMediaPlayerService mixMediaPlayerService,
        IFocusHistoryService focusHistoryService,
        ITelemetry telemetry,
        IUserSettings userSettings)
    {
        Guard.IsNotNull(timerService);
        Guard.IsNotNull(focusToastService);
        Guard.IsNotNull(mixMediaPlayerService);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(focusHistoryService);
        Guard.IsNotNull(userSettings);
        _timerService = timerService;
        _focusToastService = focusToastService;
        _mixMediaPlayerService = mixMediaPlayerService;
        _telemetry = telemetry;
        _focusHistoryService = focusHistoryService;
        _userSettings = userSettings;

        _timerService.Interval = 1000;
        _timerService.IntervalElapsed += OnIntervalElapsed;
        _mixMediaPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
    }

    public FocusSession CurrentSession { get; private set; } = new FocusSession(SessionType.None, TimeSpan.Zero, 0, 0);

    public SessionType CurrentSessionType => CurrentSession.SessionType;

    public FocusState CurrentState
    {
        get => _focusState;
        set
        {
            _focusState = value;
            FocusStateChanged?.Invoke(this, _focusState);
        }
    }

    public bool StartTimer(int focusLength, int restLength, int originalRepetitions)
    {
        if (!CanStartSession(focusLength, restLength))
        {
            return false;
        }

        _timerService.Stop();
        _sessionQueue.Clear();

        int repetitions = originalRepetitions;
        int queueSize = (repetitions + 1) * 2;
        int queuePosition = 0;

        while (repetitions >= 0)
        {
            _sessionQueue.Enqueue(new FocusSession(
                SessionType.Focus,
                TimeSpan.FromMinutes(focusLength),
                queuePosition++,
                queueSize));

            if (restLength > 0)
            {
                _sessionQueue.Enqueue(new FocusSession(
                    SessionType.Rest,
                    TimeSpan.FromMinutes(restLength),
                    queuePosition++,
                    queueSize));
            }

            repetitions -= 1;
        }

        _focusToastService.ScheduleToasts(_sessionQueue.ToArray(), DateTime.Now);

        CurrentSession = _sessionQueue.Dequeue();
        TimeUpdated?.Invoke(this, CurrentSession);
        _timerService.Remaining = CurrentSession.Remaining;
        _timerService.Start();
        CurrentState = FocusState.Active;

        PlaySounds();

        _focusHistoryService.TrackNewHistory(
            DateTime.UtcNow.Ticks,
            focusLength,
            restLength,
            originalRepetitions);

        return true;
    }

    private void PlaySounds()
    {
        var sounds = _mixMediaPlayerService.GetSoundIds();
        if (sounds.Length > 0)
        {
            _mixMediaPlayerService.Play();
        }
        else
        {
            _ = _mixMediaPlayerService.PlayRandomAsync();
        }
    }

    public bool CanStartSession(int focusLength, int restLength) => focusLength > 0 && restLength >= 0;

    public void PauseTimer()
    {
        _timerService.Stop();
        _focusToastService.ClearToasts();
        CurrentState = FocusState.Paused;
        _mixMediaPlayerService.Pause();
    }

    public void StopTimer(bool sessionCompleted = false, bool pauseSounds = true)
    {
        _timerService.Stop();
        _focusToastService.ClearToasts();

        if (sessionCompleted)
        {
            if (pauseSounds)
            {
                // When user does not want to pause sound, it's because they want to continue focusing.
                // Sending a toast would only break their concentration. So we only send this toast
                // if the user wants sounds to be paused.
                _focusToastService.SendCompletionToast();
            }

            _telemetry.TrackEvent(TelemetryConstants.FocusCompleted);
            _focusHistoryService.TrackHistoryCompletion(
                DateTime.UtcNow.Ticks,
                CurrentSession.SessionType);
        }
        else
        {
            _focusHistoryService.TrackIncompleteHistory(
                DateTime.UtcNow.Ticks,
                CurrentSession.SessionType,
                CurrentSession.OriginalLength - CurrentSession.Remaining);
        }

        CurrentSession = new FocusSession(SessionType.None, TimeSpan.Zero, 0, 0);
        TimeUpdated?.Invoke(this, CurrentSession);
        CurrentState = FocusState.None;

        if (pauseSounds)
        {
            _mixMediaPlayerService.Pause();
        }

        // Need to reset to the previous volume so that 
        // if the user starts focusing again, the volume isn't 0.
        if (_mixMediaPlayerService.GlobalVolume == 0)
        {
            _mixMediaPlayerService.GlobalVolume = _previousGlobalVolume;
        }
    }

    /// <inheritdoc/>
    public bool SkipSegment()
    {
        _skipSegmentRequested = true;

        if (CurrentState is FocusState.Paused)
        {
            ResumeTimer();
        }

        return true;
    }

    public bool ResumeTimer()
    {
        if (_timerService.Remaining > TimeSpan.Zero)
        {
            var sessions = new List<FocusSession>() { CurrentSession };
            sessions.AddRange(_sessionQueue);

            _timerService.Start();
            _focusToastService.ScheduleToasts(sessions, DateTime.Now);
            CurrentState = FocusState.Active;
            PlaySounds();
            return true;
        }

        return false;
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

        if (remaining == TimeSpan.Zero || _skipSegmentRequested)
        {
            _timerService.Stop();

            if (_skipSegmentRequested)
            {
                _skipSegmentRequested = false;

                // Reschedule toasts since we changed the original timeline.
                _focusToastService.ClearToasts();
                _focusToastService.ScheduleToasts(_sessionQueue.ToArray(), DateTime.Now);
            }

            if (_sessionQueue.Count > 0)
            {
                // One session done, more to go.
                _focusHistoryService.TrackSegmentEnd(CurrentSession.SessionType);
                CurrentSession = _sessionQueue.Dequeue();
                if (CurrentSession.SessionType == SessionType.Rest)
                {
                    _previousGlobalVolume = _mixMediaPlayerService.GlobalVolume;
                }
                _mixMediaPlayerService.GlobalVolume = CurrentSession.SessionType == SessionType.Rest ? 0 : _previousGlobalVolume;
                _timerService.Remaining = CurrentSession.Remaining;
                _timerService.Start();
                _telemetry.TrackEvent(TelemetryConstants.FocusSegmentCompleted);
            }
            else
            {
                // Whole focus session is done.
                StopTimer(sessionCompleted: true, pauseSounds: !_userSettings.Get<bool>(UserSettingsConstants.PlayAfterFocusKey));
            }
        }
    }

    private void OnPlaybackStateChanged(object sender, MediaPlaybackState e)
    {
        if (e == MediaPlaybackState.Playing && CurrentState == FocusState.Paused)
        {
            ResumeTimer();
        }
        else if (e == MediaPlaybackState.Paused && CurrentState == FocusState.Active)
        {
            PauseTimer();
        }
    }
}

public enum FocusState
{
    None,
    Active,
    Paused
}

public class FocusSession
{
    public FocusSession(SessionType sessionType, TimeSpan originalLength, int queuePosition, int queueSize)
    {
        SessionType = sessionType;
        OriginalLength = originalLength;
        Remaining = originalLength;
        QueuePosition = queuePosition;
        QueueSize = queueSize;
    }

    public SessionType SessionType { get; }

    public TimeSpan OriginalLength { get; }

    public TimeSpan Remaining { get; set; }

    public int QueueSize { get; }

    public int QueuePosition { get; }
}
