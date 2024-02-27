using AmbientSounds.Tools;
using System;

namespace AmbientSounds.Services;

public sealed class SleepTimerService : ISleepTimerService
{
    private const int DefaultTimerInterval = 1000; // ms
    private readonly IMixMediaPlayerService _player;
    private readonly ITimerService _timer;
    private double _originalTime;
    private SleepTimerState _state;

    /// <inheritdoc/>
    public event EventHandler<TimeSpan>? TimerIntervalElapsed;

    /// <inheritdoc/>
    public event EventHandler<SleepTimerState>? StateChanged;

    public SleepTimerService(IMixMediaPlayerService player, ITimerService timer)
    {
        _player = player;
        _timer = timer;
        _timer.Interval = DefaultTimerInterval;

        _timer.IntervalElapsed += OnTimerIntervalElapsed;
        _player.PlaybackStateChanged += OnPlaybackStateChanged;
    }

    /// <inheritdoc/>
    public SleepTimerState State
    {
        get => _state;
        private set
        {
            if (value != _state)
            {
                _state = value;
                StateChanged?.Invoke(this, _state);
            }
        }
    }

    /// <inheritdoc/>
    public TimeSpan TimeLeft => _timer.Remaining;

    /// <inheritdoc/>
    public double PercentLeft => _originalTime == 0
        ? 0
        : TimeLeft.TotalMinutes / _originalTime * 100;

    /// <inheritdoc/>
    public void StartTimer(int minutes)
    {
        if (minutes == 0)
        {
            StopTimer();
            return;
        }

        _originalTime = minutes;
        var timeLeft = TimeSpan.FromMinutes(minutes);
        _timer.Remaining = timeLeft;
        State = SleepTimerState.Running;

        if (_player.PlaybackState is MediaPlaybackState.Playing)
        {
            _timer.Start();
        }
    }

    /// <inheritdoc/>
    public void StopTimer()
    {
        _timer.Stop();
        _timer.Remaining = TimeSpan.Zero;
        _originalTime = 0;
        State = SleepTimerState.Off;
    }

    /// <inheritdoc/>
    public void ResumeTimer()
    {
        if (_timer.Remaining > TimeSpan.Zero)
        {
            _timer.Start();
            State = SleepTimerState.Running;
        }
    }

    /// <inheritdoc/>
    public void PauseTimer()
    {
        _timer.Stop();
        State = SleepTimerState.Paused;
    }

    private void OnTimerIntervalElapsed(object sender, TimeSpan remaining)
    {
        if (remaining < TimeSpan.FromSeconds(1))
        {
            StopTimer();
            _player.Pause();
        }

        TimerIntervalElapsed?.Invoke(sender, remaining);
    }

    private void OnPlaybackStateChanged(object sender, MediaPlaybackState e)
    {
        if (State is SleepTimerState.Off)
        {
            return;
        }

        if (e is MediaPlaybackState.Paused)
        {
            PauseTimer();
        }
        else if (e is MediaPlaybackState.Opening or MediaPlaybackState.Playing)
        {
            ResumeTimer();
        }
    }
}

public enum SleepTimerState
{
    Off,
    Running,
    Paused,
}
