using System;
using System.Collections.Generic;
using System.Timers;
using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;

namespace AmbientSounds.ViewModels;

public partial class DigitalClockViewModel : ObservableObject
{
    private readonly IUserSettings _userSettings;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly ITimerService _countdownTimer;
    private readonly ITelemetry _telemetry;
    private readonly IFocusService _focusService;
    private readonly Timer _clockTimer = new()
    {
        Interval = 1000 // milliseconds
    };

    public DigitalClockViewModel(
        IUserSettings userSettings,
        IDispatcherQueue dispatcherQueue,
        ITimerService timerService,
        ITelemetry telemetry,
        IFocusService focusService)
    {
        _userSettings = userSettings;
        _dispatcherQueue = dispatcherQueue;
        _countdownTimer = timerService;
        _telemetry = telemetry;
        _focusService = focusService;

        _countdownTimer.Interval = 1000; // milliseconds
    }

    [ObservableProperty]
    private int _hoursInput;

    [ObservableProperty]
    private int _minutesInput;

    [ObservableProperty]
    private int _secondsInput;

    [ObservableProperty]
    private string _timeText = string.Empty;

    [ObservableProperty]
    private string _countdownText = string.Empty;

    [ObservableProperty]
    private bool _showClock;

    /// <summary>
    /// Boolean to track if the countdown is enabled via settings.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountdownTimerVisible))]
    [NotifyPropertyChangedFor(nameof(CountdownInputVisible))]
    [NotifyPropertyChangedFor(nameof(ShowCountdown))]
    private bool _countdownEnabled;

    /// <summary>
    /// Determines if the countdown control should be shown.
    /// </summary>
    public bool ShowCountdown => CountdownEnabled && _focusService.CurrentState is FocusState.None;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountdownTimerVisible))]
    [NotifyPropertyChangedFor(nameof(CountdownInputVisible))]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    private bool _countdownActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    private bool _countdownPaused;

    public bool CanStart => !CountdownActive || CountdownPaused;

    public bool CountdownTimerVisible => ShowCountdown && CountdownActive;

    public bool CountdownInputVisible => ShowCountdown && !CountdownActive;

    public void Initialize()
    {
        _clockTimer.Elapsed += OnTimerElapsed;
        _userSettings.SettingSet += OnSettingSet;
        _countdownTimer.IntervalElapsed += OnCountdownIntervalElapsed;
        _countdownTimer.Remaining = new TimeSpan(2, 0, 0);

        CountdownEnabled = _userSettings.Get<string>(UserSettingsConstants.ChannelTimerModeKey) == ChannelTimerMode.Countdown.ToString();
        ShowClock = _userSettings.Get<bool>(UserSettingsConstants.ChannelClockEnabledKey);

        if (ShowClock)
        {
            UpdateTimeText();
            _clockTimer.Start();
        }

        if (ShowCountdown)
        {
            UpdateCountdownText();
        }
    }

    public void Uninitialize()
    {
        _clockTimer.Stop();
        _clockTimer.Elapsed -= OnTimerElapsed;
        _userSettings.SettingSet -= OnSettingSet;
        ResetTimerCommand.Execute(null);
        _countdownTimer.IntervalElapsed -= OnCountdownIntervalElapsed;
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        UpdateTimeText();
    }

    private void OnCountdownIntervalElapsed(object sender, TimeSpan remaining)
    {
        if (remaining.TotalMilliseconds <= 0)
        {
            _dispatcherQueue.TryEnqueue(ResetTimer);
        }

        UpdateCountdownText();
    }

    private void OnSettingSet(object sender, string settingKey)
    {
        if (settingKey == UserSettingsConstants.ChannelClockEnabledKey)
        {
            ShowClock = _userSettings.Get<bool>(UserSettingsConstants.ChannelClockEnabledKey);
            if (ShowClock)
            {
                UpdateTimeText();
                _clockTimer.Start();
            }
            else
            {
                _clockTimer.Stop();
            }
        }
        else if (settingKey == UserSettingsConstants.ChannelTimerModeKey)
        {
            CountdownEnabled = _userSettings.Get<string>(UserSettingsConstants.ChannelTimerModeKey) == ChannelTimerMode.Countdown.ToString();

            if (ShowCountdown)
            {
                UpdateCountdownText();
            }
            else
            {
                ResetTimerCommand.Execute(null);
            }
        }
    }

    private void UpdateTimeText()
    {
        if (!ShowClock)
        {
            return;
        }

        _dispatcherQueue.TryEnqueue(() =>
        {
            TimeText = DateTime.Now.ToShortTimeString();
        });
    }

    private void UpdateCountdownText()
    {
        if (!ShowCountdown)
        {
            return;
        }

        _dispatcherQueue.TryEnqueue(() =>
        {
            CountdownText = _countdownTimer.Remaining.ToString("g");
        });
    }

    [RelayCommand]
    private void StartTimer()
    {
        if (CountdownPaused)
        {
            _countdownTimer.Start();
            CountdownPaused = false;
            return;
        }

        if (CanStart &&
            (HoursInput > 0 || MinutesInput > 0 || SecondsInput > 0))
        {
            _countdownTimer.Remaining = new TimeSpan(HoursInput, MinutesInput, SecondsInput);
            UpdateCountdownText();
            _countdownTimer.Start();
            CountdownActive = true;
            CountdownPaused = false;
            _telemetry.TrackEvent(TelemetryConstants.ChannelViewerCountdownStarted, new Dictionary<string, string>
            {
                { "userInput", $"{HoursInput}:{MinutesInput}:{SecondsInput}" }
            });
        }
    }

    [RelayCommand]
    private void ResetTimer()
    {
        _countdownTimer.Stop();
        CountdownActive = false;
        CountdownPaused = false;
    }

    [RelayCommand]
    private void PauseTimer()
    {
        if (!CountdownActive || CountdownPaused)
        {
            return;
        }

        _countdownTimer.Stop();
        CountdownPaused = true;
    }

    [RelayCommand]
    private void ToggleTimer()
    {
        if (CountdownPaused || !CountdownActive)
        {
            StartTimerCommand.Execute(null);
        }
        else
        {
            PauseTimerCommand.Execute(null);
        }
    }
}
