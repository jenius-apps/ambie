using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Humanizer.Localisation;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AmbientSounds.ViewModels;

public partial class SleepTimerViewModel : ObservableObject
{
    private readonly ISleepTimerService _sleepTimerService;
    private readonly ITelemetry _telemetry;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly ILocalizer _localizer;
    private readonly int[] _timeOptions = [15, 30, 45, 60, 90, 120];
    private SleepTimerOptionsViewModel? _activeTimerOption;

    public SleepTimerViewModel(
        ISleepTimerService sleepTimerService, 
        ITelemetry telemetry,
        IDispatcherQueue dispatcherQueue,
        ILocalizer localizer)
    {
        _sleepTimerService = sleepTimerService;
        _telemetry = telemetry;
        _dispatcherQueue = dispatcherQueue;
        _localizer = localizer;

        Options.Add(new SleepTimerOptionsViewModel(0, _localizer.GetString("OffTextBlock/Text"), StartTimerCommand));

        foreach (var option in _timeOptions)
        {
            Options.Add(new SleepTimerOptionsViewModel(option, TimeSpan.FromMinutes(option).Humanize(maxUnit: TimeUnit.Minute), StartTimerCommand));
        }

        Options[0].IsActive = true;
    }

    /// <summary>
    /// Determines if the sleep timer's countdown is visible.
    /// </summary>
    [ObservableProperty]
    private bool _countdownVisible;

    /// <summary>
    /// String representation of time remaining.
    /// E.g. 0:59:12 for 59 minutes and 12 seconds left.
    /// </summary>
    [ObservableProperty]
    private string _timeLeft = string.Empty;

    /// <summary>
    /// Time left in the sleep timer in the form of a
    /// decrementing percentage. E.g. 100 > 99 > 98...
    /// </summary>
    [ObservableProperty]
    private double _percentLeft;

    public ObservableCollection<SleepTimerOptionsViewModel> Options { get; } = [];
    
    [RelayCommand]
    private void StartTimer(int minutes)
    {
        if (minutes == 0 || _activeTimerOption?.Minutes == minutes)
        {
            StopTimer();
            return;
        }

        foreach (var option in Options)
        {
            option.IsActive = option.Minutes == minutes;

            if (option.IsActive)
            {
                _activeTimerOption = option;
            }
        }

        _sleepTimerService.StartTimer(minutes);
        UpdateTimeLeft();
        CountdownVisible = true;

        _telemetry.TrackEvent(TelemetryConstants.TimeSelected, new Dictionary<string, string>
        {
            { "length", minutes.ToString() }
        });
    }

    [RelayCommand]
    private void StopTimer()
    {
        _activeTimerOption = null;
        foreach (var option in Options)
        {
            option.IsActive = option.Minutes == 0;
        }

        _sleepTimerService.StopTimer();
        UpdateTimeLeft();
        CountdownVisible = false;
    }

    private void TimerElapsed(object sender, TimeSpan remaining)
    {
        _dispatcherQueue.TryEnqueue(UpdateTimeLeft);
    }

    public void Initialize()
    {
        _sleepTimerService.TimerIntervalElapsed += TimerElapsed;
        _sleepTimerService.StateChanged += StateChanged;
    }

    public void Uninitialize()
    {
        _sleepTimerService.TimerIntervalElapsed -= TimerElapsed;
        _sleepTimerService.StateChanged -= StateChanged;
    }

    private void UpdateTimeLeft()
    {
        TimeLeft = _sleepTimerService.TimeLeft.ToString("g");
        PercentLeft = _sleepTimerService.PercentLeft;
    }

    private void StateChanged(object sender, SleepTimerState e)
    {
        if (e is SleepTimerState.Off)
        {
            CountdownVisible = false;
        }
    }
}

public sealed partial class SleepTimerOptionsViewModel : ObservableObject
{
    private readonly IRelayCommand<int> _startCommand;

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
