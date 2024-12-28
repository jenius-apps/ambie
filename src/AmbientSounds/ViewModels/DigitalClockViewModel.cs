using System;
using System.Timers;
using AmbientSounds.Constants;
using AmbientSounds.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Tools;

namespace AmbientSounds.ViewModels;

public partial class DigitalClockViewModel : ObservableObject
{
    private readonly IUserSettings _userSettings;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly ITimerService _countdownTimer;
    private readonly Timer _clockTimer = new()
    {
        Interval = 1000 // milliseconds
    };

    public DigitalClockViewModel(
        IUserSettings userSettings,
        IDispatcherQueue dispatcherQueue,
        ITimerService timerService)
    {
        _userSettings = userSettings;
        _dispatcherQueue = dispatcherQueue;
        _countdownTimer = timerService;

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
    private bool _showSeconds;

    [ObservableProperty]
    private bool _showClock;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountdownTimerVisible))]
    [NotifyPropertyChangedFor(nameof(CountdownInputVisible))]
    private bool _showCountdown;

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

        ShowCountdown = _userSettings.Get<bool>(UserSettingsConstants.ChannelCountdownEnabledKey);
        ShowSeconds = _userSettings.Get<bool>(UserSettingsConstants.ChannelClockSecondsEnabledKey);
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
        _countdownTimer.Stop();
        _countdownTimer.IntervalElapsed -= OnCountdownIntervalElapsed;
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        UpdateTimeText();
    }

    private void OnCountdownIntervalElapsed(object sender, TimeSpan e)
    {
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
        else if (settingKey == UserSettingsConstants.ChannelClockSecondsEnabledKey)
        {
            ShowSeconds = _userSettings.Get<bool>(UserSettingsConstants.ChannelClockSecondsEnabledKey);
            UpdateTimeText();
        }
        else if (settingKey == UserSettingsConstants.ChannelCountdownEnabledKey)
        {
            ShowCountdown = _userSettings.Get<bool>(UserSettingsConstants.ChannelCountdownEnabledKey);

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
            TimeText = ShowSeconds ? DateTime.Now.ToLongTimeString() : DateTime.Now.ToShortTimeString();
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
