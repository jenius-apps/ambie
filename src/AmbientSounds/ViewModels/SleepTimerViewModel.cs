using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;

namespace AmbientSounds.ViewModels;

public partial class SleepTimerViewModel : ObservableObject
{
    private const int DefaultTimerInterval = 1000; // ms
    private readonly IMixMediaPlayerService _player;
    private readonly ITelemetry _telemetry;
    private readonly ITimerService _timer;
    private readonly IDispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private bool _playVisible;

    [ObservableProperty]
    private bool _stopVisible;

    /// <summary>
    /// Determines if the sleep timer's countdown is visible.
    /// </summary>
    [ObservableProperty]
    private bool _countdownVisible;

    public SleepTimerViewModel(
        IMixMediaPlayerService player,
        ITimerService timer,
        ITelemetry telemetry,
        IDispatcherQueue dispatcherQueue)
    {
        Guard.IsNotNull(player);
        Guard.IsNotNull(timer);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(dispatcherQueue);

        _player = player;
        _timer = timer;
        _telemetry = telemetry;
        _dispatcherQueue = dispatcherQueue;
        _timer.Interval = DefaultTimerInterval;
    }
    
    private void OnPlaybackStateChanged(object sender, MediaPlaybackState e)
    {
        if (e == MediaPlaybackState.Paused)
            PauseTimer();
        else if (e == MediaPlaybackState.Opening || e == MediaPlaybackState.Playing)
            PlayTimer();
    }

    /// <summary>
    /// String representation of time remaining.
    /// E.g. 0:59:12 for 59 minutes and 12 seconds left.
    /// </summary>
    public string TimeLeft => _timer.Remaining.ToString("g");

    [RelayCommand]
    private void StartTimer(int minutes)
    {
        _telemetry.TrackEvent(TelemetryConstants.TimeSelected, new Dictionary<string, string>
        {
            { "length", minutes.ToString() }
        });

        _timer.Remaining = TimeSpan.FromMinutes(minutes);
        OnPropertyChanged(nameof(TimeLeft));
        CountdownVisible = true;
        _timer.Start();
        StopVisible = true;
        PlayVisible = false;
    }

    [RelayCommand]
    private void PlayTimer()
    {
        if (_timer.Remaining > TimeSpan.Zero)
        {
            _timer.Start();
            StopVisible = true;
            PlayVisible = false;
        }
    }

    [RelayCommand]
    private void PauseTimer()
    {
        _timer.Stop();
        StopVisible = false;

        if (_timer.Remaining > TimeSpan.Zero)
        {
            PlayVisible = true;
        }
    }

    [RelayCommand]
    private void StopTimer()
    {
        _timer.Stop();
        _timer.Remaining = TimeSpan.Zero;
        OnPropertyChanged(nameof(TimeLeft));
        CountdownVisible = false;
        StopVisible = false;
        PlayVisible = false;
    }

    private void TimerElapsed(object sender, TimeSpan remaining)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(TimeLeft));
            if (remaining < TimeSpan.FromSeconds(1))
            {
                StopTimer();
                _player.Pause();
            }
        });
    }

    public void Initialize()
    {
        _timer.IntervalElapsed += TimerElapsed;
        _player.PlaybackStateChanged += OnPlaybackStateChanged;
    }

    public void Dispose()
    {
        _timer.IntervalElapsed -= TimerElapsed;
        _player.PlaybackStateChanged -= OnPlaybackStateChanged;
    }
}
