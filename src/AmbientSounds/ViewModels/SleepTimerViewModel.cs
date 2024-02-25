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
using System.Collections.ObjectModel;

namespace AmbientSounds.ViewModels;

public sealed partial class SleepTimerOptionsViewModel : ObservableObject
{
    private IRelayCommand<int> _startCommand { get; }

    public SleepTimerOptionsViewModel(
        int minutes,
        string label,
        IRelayCommand<int> startCommand)
    {
        Minutes = minutes;
        Label = label;
        _startCommand = startCommand;
    }

    [ObservableProperty]
    private bool _isActive;

    public string Label { get; } = string.Empty;

    public int Minutes { get; }

    [RelayCommand]
    public void Start()
    {
        _startCommand.Execute(Minutes);
    }
}

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
        _player = player;
        _timer = timer;
        _telemetry = telemetry;
        _dispatcherQueue = dispatcherQueue;
        _timer.Interval = DefaultTimerInterval;

        Options.Add(new SleepTimerOptionsViewModel(0, "Off", StartTimerCommand));
        Options.Add(new SleepTimerOptionsViewModel(15, "15 min", StartTimerCommand));
        Options.Add(new SleepTimerOptionsViewModel(30, "30 min", StartTimerCommand));

        Options[0].IsActive = true;
    }

    /// <summary>
    /// String representation of time remaining.
    /// E.g. 0:59:12 for 59 minutes and 12 seconds left.
    /// </summary>
    [ObservableProperty]
    private string _timeLeft = string.Empty;

    public ObservableCollection<SleepTimerOptionsViewModel> Options { get; } = [];
    
    private void OnPlaybackStateChanged(object sender, MediaPlaybackState e)
    {
        if (e == MediaPlaybackState.Paused)
            PauseTimer();
        else if (e == MediaPlaybackState.Opening || e == MediaPlaybackState.Playing)
            PlayTimer();
    }

    [RelayCommand]
    private void StartTimer(int minutes)
    {
        if (minutes == 0)
        {
            StopTimer();
            return;
        }

        foreach (var option in Options)
        {
            option.IsActive = option.Minutes == minutes;
        }

        _telemetry.TrackEvent(TelemetryConstants.TimeSelected, new Dictionary<string, string>
        {
            { "length", minutes.ToString() }
        });

        var timeLeft = TimeSpan.FromMinutes(minutes);
        _timer.Remaining = timeLeft;
        UpdateTimeLeft(timeLeft);
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
        foreach (var option in Options)
        {
            option.IsActive = option.Minutes == 0;
        }

        _timer.Stop();
        _timer.Remaining = TimeSpan.Zero;
        UpdateTimeLeft(TimeSpan.Zero);
        CountdownVisible = false;
        StopVisible = false;
        PlayVisible = false;
    }

    private void TimerElapsed(object sender, TimeSpan remaining)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            UpdateTimeLeft(remaining);
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

    public void Uninitialize()
    {
        _timer.IntervalElapsed -= TimerElapsed;
        _player.PlaybackStateChanged -= OnPlaybackStateChanged;
    }

    private void UpdateTimeLeft(TimeSpan timeLeft)
    {
        TimeLeft = timeLeft == TimeSpan.Zero
            ? string.Empty
            : timeLeft.ToString("g");
    }
}
