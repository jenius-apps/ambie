using System;
using System.Timers;
using AmbientSounds.Constants;
using AmbientSounds.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
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
    private string _timeText = string.Empty;

    [ObservableProperty]
    private string _countdownText = string.Empty;

    [ObservableProperty]
    private bool _showSeconds;

    [ObservableProperty]
    private bool _showClock;

    [ObservableProperty]
    private bool _showCountdown;

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
            _countdownTimer.Start();
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
}
