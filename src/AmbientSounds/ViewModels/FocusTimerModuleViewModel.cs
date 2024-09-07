using AmbientSounds.Constants;
using AmbientSounds.Extensions;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using INavigator = AmbientSounds.Services.INavigator;

namespace AmbientSounds.ViewModels;

public partial class FocusSegmentViewModel : ObservableObject
{
    [ObservableProperty]
    private double _progress;
}

public partial class FocusTimerModuleViewModel : ObservableObject
{
    private readonly IFocusService _focusService;
    private readonly IUserSettings _userSettings;
    private readonly ILocalizer _localizer;
    private readonly ITelemetry _telemetry;
    private readonly IRecentFocusService _recentFocusService;
    private readonly IFocusHistoryService _focusHistoryService;
    private readonly IFocusTaskService _taskService;
    private readonly INavigator _navigator;
    private readonly ISystemInfoProvider _systemInfoProvider;
    private readonly IDialogService _dialogService;
    private readonly ICompactNavigator _compactNavigator;
    private readonly IDispatcherQueue _dispatcherQueue;
    private bool _isHelpMessageVisible;
    private int _focusLength;
    private int _restLength;
    private int _repetitions;

    [ObservableProperty]
    private bool _isResting;
    [ObservableProperty]
    private bool _isFocusing;
    [ObservableProperty]
    private bool _secondsRingVisible;
    [ObservableProperty]
    private int _secondsRemaining;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FocusLengthProgress))]
    private double _focusLengthRemaining; 
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RestLengthProgress))]
    private double _restLengthRemaining;
    [ObservableProperty]
    private int _repetitionsRemaining;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHelpIconVisible))]
    private bool _slidersEnabled;
    [ObservableProperty]
    private bool _playEnabled;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TasksVisible))]
    [NotifyPropertyChangedFor(nameof(ActiveDataVisible))]
    [NotifyPropertyChangedFor(nameof(CountdownVisible))]
    private bool _cancelVisible;
    [ObservableProperty]
    private bool _playVisible;
    [ObservableProperty]
    private bool _pauseVisible;
    [ObservableProperty]
    private string _primaryButtonText = string.Empty;
    [ObservableProperty]
    private string _currentTimeRemaining = string.Empty;
    [ObservableProperty]
    private string _originalSegmentLength = string.Empty;
    [ObservableProperty]
    private string _timeElapsed = string.Empty;
    [ObservableProperty]
    private string _currentStatus = string.Empty;
    [ObservableProperty]
    private int _selectedTaskIndex = -1;

    public FocusTimerModuleViewModel(
        IFocusService focusService,
        ILocalizer localizer,
        IRecentFocusService recentFocusService,
        ITelemetry telemetry,
        IFocusHistoryService focusHistoryService,
        IUserSettings userSettings,
        IFocusTaskService taskService,
        INavigator navigator,
        ISystemInfoProvider systemInfoProvider,
        IDialogService dialogService,
        ICompactNavigator compactNavigator,
        IDispatcherQueue dispatcherQueue)
    {
        _focusService = focusService;
        _userSettings = userSettings;
        _localizer = localizer;
        _telemetry = telemetry;
        _recentFocusService = recentFocusService;
        _focusHistoryService = focusHistoryService;
        _taskService = taskService;
        _navigator = navigator;
        _systemInfoProvider = systemInfoProvider;
        _dialogService = dialogService;
        _compactNavigator = compactNavigator;
        _dispatcherQueue = dispatcherQueue;
        IsHelpMessageVisible = !userSettings.Get<bool>(UserSettingsConstants.HasClosedFocusHelpMessageKey);
        UpdateButtonStates();
        InterruptionCommand = new AsyncRelayCommand(LogInterruptionAsync);
    }

    [ObservableProperty]
    private bool _skipSegmentRequested;

    [ObservableProperty]
    private bool _insightsVisible;

    public ObservableCollection<RecentFocusSettingsViewModel> RecentSettings { get; } = new();

    public ObservableCollection<FocusTaskViewModel> FocusTasks { get; } = new();

    public ObservableCollection<FocusSegmentViewModel> Segments { get; } = new();

    public double FocusLengthProgress => FocusLength - FocusLengthRemaining;

    public double RestLengthProgress => RestLength - RestLengthRemaining;

    public bool TasksVisible => CancelVisible && FocusTasks.Count > 0;

    public bool ActiveDataVisible => CancelVisible && FocusTasks.Count == 0;

    public bool CountdownVisible => CancelVisible && FocusTasks.Count == 0 ;

    public bool IsRecentVisible => _focusService.CurrentState == FocusState.None && RecentSettings.Count > 0;

    public IAsyncRelayCommand InterruptionCommand { get; }

    public int InterruptionCount => _focusHistoryService.GetInterruptionCount();

    public int Pauses => _focusHistoryService.GetPauses();

    public bool IsHelpIconVisible => !IsHelpMessageVisible && SlidersEnabled;

    public bool IsHelpMessageVisible
    {
        get => _isHelpMessageVisible;
        set
        {
            SetProperty(ref _isHelpMessageVisible, value);
            if (value is false)
            {
                _userSettings.Set(UserSettingsConstants.HasClosedFocusHelpMessageKey, true);
            }
            OnPropertyChanged(nameof(IsHelpIconVisible));
        }
    }

    public bool UseCompactMode
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.CompactOnFocusKey);
        set => _userSettings.Set(UserSettingsConstants.CompactOnFocusKey, value);
    }

    public bool PlayAfterFocusEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.PlayAfterFocusKey);
        set => _userSettings.Set(UserSettingsConstants.PlayAfterFocusKey, value);
    }

    public int FocusLength
    {
        get => _focusLength;
        set
        {
            SetProperty(ref _focusLength, value);
            OnPropertyChanged(nameof(TotalTime));
            OnPropertyChanged(nameof(TotalFocus));
            OnPropertyChanged(nameof(EndTime));
            UpdatePlayEnabled();
            FocusLengthRemaining = value;
        }
    }

    public int RestLength
    {
        get => _restLength;
        set
        {
            SetProperty(ref _restLength, value);
            OnPropertyChanged(nameof(TotalTime));
            OnPropertyChanged(nameof(TotalRest));
            OnPropertyChanged(nameof(EndTime));
            UpdatePlayEnabled();
            RestLengthRemaining = value;
        }
    }

    public int Repetitions
    {
        get => _repetitions;
        set
        {
            SetProperty(ref _repetitions, value);
            OnPropertyChanged(nameof(TotalTime));
            OnPropertyChanged(nameof(EndTime));
            OnPropertyChanged(nameof(TotalFocus));
            OnPropertyChanged(nameof(TotalRest));
            RepetitionsRemaining = value;
        }
    }

    public string TotalTime
    {
        get
        {
            TimeSpan time = FocusExtensions.GetTotalTime(FocusLength, RestLength, Repetitions);
            return time.ToString(@"hh\:mm");
        }
    }

    public string TotalFocus
    {
        get
        {
            TimeSpan time = FocusExtensions.GetTotalTime(FocusLength, 0, Repetitions);
            return time.ToString(@"hh\:mm");
        }
    }

    public string TotalRest
    {
        get
        {
            TimeSpan time = FocusExtensions.GetTotalTime(0, RestLength, Repetitions);
            return time.ToString(@"hh\:mm");
        }
    }

    public string StartTime => _focusHistoryService.GetStartTime().ToShortTimeString();

    public string EndTime
    {
        get
        {
            TimeSpan duration = FocusExtensions.GetTotalTime(FocusLength, RestLength, Repetitions);
            DateTime endTime = DateTime.Now.Add(duration);
            return endTime.ToShortTimeString();
        }
    }

    public async Task InitializeAsync()
    {
        _focusService.TimeUpdated += OnTimeUpdated;
        _focusService.FocusStateChanged += OnFocusStateChanged;
        _userSettings.SettingSet += OnSettingChanged;

        await InitializeTasksAsync();
        InitializeSegments();
        var recentInterruptionTask = _focusHistoryService.GetRecentInterruptionsAsync();

        // Initialize recent list
        if (RecentSettings.Count > 0)
        {
            RecentSettings.Clear();
        }

        var recents = await _recentFocusService.GetRecentAsync();
        foreach (var recent in recents.OrderByDescending(x => x.LastUsed))
        {
            RecentSettings.Add(new RecentFocusSettingsViewModel(recent, DeleteRecentSettingCommand));
        }

        if (RecentSettings.FirstOrDefault() is { } s)
        {
            LoadRecentSettings(s);
        }

        var interruptions = await recentInterruptionTask;
        InsightsVisible = interruptions.Count > 0;
        OnPropertyChanged(nameof(IsRecentVisible));

        UpdateButtonStates();
    }

    public void Uninitialize()
    {
        _focusService.TimeUpdated -= OnTimeUpdated;
        _focusService.FocusStateChanged -= OnFocusStateChanged;
        _userSettings.SettingSet -= OnSettingChanged;

        SelectedTaskIndex = 0;
        InsightsVisible = false;
        RecentSettings.Clear();
        FocusTasks.Clear();
        Segments.Clear();
    }

    public bool CanStartTutorial() => SlidersEnabled;

    public void PlayOrPause()
    {
        if (PauseVisible)
        {
            Pause();
        }
        else if (PlayVisible)
        {
            Start();
        }
    }

    private void Pause()
    {
        _focusService.PauseTimer();
    }

    public void Stop()
    {
        Segments.Clear();
        _focusService.StopTimer();
    }

    public void ShowHelpMessage()
    {
        IsHelpMessageVisible = true;
    }

    public void LoadRecentSettings(RecentFocusSettingsViewModel vm)
    {
        if (vm is null)
        {
            return;
        }

        FocusLength = vm.Settings.FocusMinutes;
        RestLength = vm.Settings.RestMinutes;
        Repetitions = vm.Settings.Repeats;
    }

    [RelayCommand]
    public async Task DeleteRecentSettingAsync(RecentFocusSettingsViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        RecentSettings.Remove(vm);
        await _recentFocusService.RemoveRecentAsync(vm.Settings);
    }

    [RelayCommand]
    private void CompleteTask(FocusTaskViewModel? task)
    {
        if (task is null)
        {
            return;
        }

        _focusHistoryService.LogTaskCompleted(task.Task.Id);
        _ = _taskService.UpdateCompletionAsync(task.Task.Id, true).ConfigureAwait(false);
        _telemetry.TrackEvent(TelemetryConstants.TaskCompleted, new Dictionary<string, string>
        {
            { "inSession", "true" }
        });
    }

    [RelayCommand]
    private void ReopenTask(FocusTaskViewModel? task)
    {
        if (task is null)
        {
            return;
        }

        _focusHistoryService.RevertTaskCompleted(task.Task.Id);
        _ = _taskService.UpdateCompletionAsync(task.Task.Id, false).ConfigureAwait(false);
    }

    private void OnFocusStateChanged(object sender, FocusState e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(IsRecentVisible));
            UpdateButtonStates();
            if (e == FocusState.Paused)
            {
                _focusHistoryService.LogPause();
                OnPropertyChanged(nameof(Pauses));
            }
        });
    }

    private void OnSettingChanged(object sender, string settingKey)
    {
        if (settingKey == UserSettingsConstants.CompactOnFocusKey)
        {
            OnPropertyChanged(nameof(UseCompactMode));
        }
    }

    private async void Start()
    {
        bool successfullyStarted;

        if (_focusService.CurrentState == FocusState.Paused)
        {
            successfullyStarted = _focusService.ResumeTimer();
            OnPropertyChanged(nameof(EndTime));
        }
        else
        {
            await InitializeTasksAsync();
            SecondsRemaining = 60;
            successfullyStarted = _focusService.StartTimer(FocusLength, RestLength, Repetitions);
            _telemetry.TrackEvent(TelemetryConstants.FocusStarted, new Dictionary<string, string>
            {
                { "focusLength", FocusLength.ToString() },
                { "restLenth", RestLength.ToString() },
                { "repetitions", Repetitions.ToString() },
                { "hourOfDay", DateTime.Now.Hour.ToString() }
            });

            if (successfullyStarted)
            {
                // We only want to perform these actions
                // when starting a new session, not when resuming.
                _ = TriggerCompactModeAsync();
                _ = _recentFocusService.AddRecentAsync(FocusLength, RestLength, Repetitions);
                InitializeSegments();
                OnPropertyChanged(nameof(StartTime));
            }
        }

        if (successfullyStarted)
        {
            // Note that if the timer
            // didn't start successfully, it does not mean
            // we should hide the seconds ring. We don't care what the current state
            // is. All we care is that if it was started,
            // make sure the ring is visible.
            SecondsRingVisible = true;
        }
    }

    private void InitializeSegments()
    {
        Segments.Clear();
        if (_focusService.CurrentState == FocusState.None)
        {
            return;
        }

        int activePosition = (int)Math.Floor(_focusService.CurrentSession.QueuePosition / 2d);
        int index = 0;
        while (index < (Repetitions + 1))
        {
            double progress = 0;
            if (index < activePosition || 
                (index == activePosition && _focusService.CurrentSession.SessionType == SessionType.Rest))
            {
                progress = 100;
            }
            else if (index == activePosition)
            {
                progress = _focusService.CurrentState == FocusState.Paused
                    ? _focusService.CurrentSession.GetPercentComplete()
                    : 0;
            }
                    
            Segments.Add(new FocusSegmentViewModel()
            {
                Progress = progress
            });
            index++;
        }
    }

    private async Task TriggerCompactModeAsync()
    {
        if (_systemInfoProvider.GetDeviceFamily() != "Windows.Desktop")
        {
            return;
        }

        if (_userSettings.Get<bool>(UserSettingsConstants.CompactOnFocusKey))
        {
            await _navigator.ToCompactOverlayAsync(CompactViewMode.Focus);
        }
    }

    private async Task InitializeTasksAsync()
    {
        FocusTasks.Clear();
        var tasks = await _taskService.GetTasksAsync();
        int index = 1;
        foreach (var t in tasks)
        {
            FocusTasks.Add(new FocusTaskViewModel(
                t, 
                complete: CompleteTaskCommand,
                reopen: ReopenTaskCommand,
                displayTitle: _localizer.GetString("TaskTitle", index.ToString())));
            index++;
        }

        if (FocusTasks.Count > 0)
        {
            SelectedTaskIndex = 0;
        }
    }

    public async Task<bool> AddTaskAsync(string task)
    {
        FocusTask? focusTask = await _taskService.AddTaskAsync(task);
        if (focusTask is null)
        {
            return false;
        }

        int index = FocusTasks.Count + 1;
        FocusTasks.Add(new FocusTaskViewModel(
            focusTask,
            complete: CompleteTaskCommand,
            reopen: ReopenTaskCommand,
            displayTitle: _localizer.GetString("TaskTitle", index.ToString())));

        SelectedTaskIndex = FocusTasks.Count - 1;
        return true;
    }

    private void UpdatePlayEnabled()
    {
        PlayEnabled = _focusService.CanStartSession(FocusLength, RestLength);
    }

    private void UpdateButtonStates()
    {
        SlidersEnabled = _focusService.CurrentState == FocusState.None;
        PlayVisible = _focusService.CurrentState == FocusState.Paused || _focusService.CurrentState == FocusState.None;
        PauseVisible = _focusService.CurrentState == FocusState.Active;
        CancelVisible = _focusService.CurrentState == FocusState.Active || _focusService.CurrentState == FocusState.Paused;
        PrimaryButtonText = PauseVisible
            ? _localizer.GetString("Pause")
            : _localizer.GetString("Start");

        // Need to update here in case session type changed.
        OnPropertyChanged(nameof(TasksVisible));
        OnPropertyChanged(nameof(ActiveDataVisible));
        OnPropertyChanged(nameof(CountdownVisible));
    }

    [RelayCommand]
    private void SkipSegment()
    {
        if (SkipSegmentRequested)
        {
            return;
        }

        SkipSegmentRequested = _focusService.SkipSegment();
        _telemetry.TrackEvent(TelemetryConstants.FocusSkipClicked);
    }

    [RelayCommand]
    private void CompactInterruption()
    {
        _compactNavigator.NavigateTo(CompactViewMode.Interruption);
    }

    private async Task LogInterruptionAsync()
    {
        (double minutes, string notes) = await _dialogService.OpenInterruptionAsync();
        _focusHistoryService.LogInterruption(minutes, notes);

        if (minutes > 0)
        {
            OnPropertyChanged(nameof(InterruptionCount));
            _telemetry.TrackEvent(
                TelemetryConstants.FocusInterruptionLogged,
                _focusHistoryService.GatherInterruptionTelemetry(minutes, notes, false));
        }
    }

    [RelayCommand]
    private async Task OpenCompactModeAsync()
    {
        if (_systemInfoProvider.GetDeviceFamily() != "Windows.Desktop")
        {
            return;
        }

        await _navigator.ToCompactOverlayAsync(CompactViewMode.Focus);
    }

    [RelayCommand]
    private async Task OpenInterruptionsAsync()
    {
        await _dialogService.RecentInterruptionsAsync();
    }

    private void OnTimeUpdated(object sender, FocusSession e)
    {
        _dispatcherQueue.TryEnqueue(() => HandleTimeUpdated(e));
    }

    private void HandleTimeUpdated(FocusSession e)
    {
        if (e.SessionType == SessionType.None)
        {
            // reset
            SecondsRemaining = 0;
            SecondsRingVisible = false;
            FocusLengthRemaining = FocusLength;
            RestLengthRemaining = RestLength;
            RepetitionsRemaining = Repetitions;
            CurrentTimeRemaining = string.Empty;
            TimeElapsed = string.Empty;
            OriginalSegmentLength = string.Empty;
            CurrentStatus = string.Empty;
            IsFocusing = false;
            IsResting = false;
            UpdateButtonStates();

            return;
        }

        int targetPosition = e.QueuePosition / 2;
        if (e.SessionType == SessionType.Focus &&
            e.QueuePosition % 2 == 0 &&
            Segments.Count > targetPosition)
        {
            Segments[targetPosition].Progress = e.GetPercentComplete();
        }

        CurrentTimeRemaining = e.Remaining.ToCountdownFormat();
        TimeElapsed = (e.OriginalLength - e.Remaining).ToCountdownFormat();
        OriginalSegmentLength = e.OriginalLength.ToCountdownFormat();
        CurrentStatus = e.SessionType.ToDisplayString(_localizer);

        if (e.SessionType == SessionType.Focus)
        {
            FocusLengthRemaining = e.Remaining.TotalMinutes;
            RestLengthRemaining = RestLength;
        }
        else if (e.SessionType == SessionType.Rest)
        {
            FocusLengthRemaining = 0;
            RestLengthRemaining = e.Remaining.TotalMinutes;
        }

        IsResting = e.SessionType == SessionType.Rest;
        IsFocusing = e.SessionType == SessionType.Focus;
        RepetitionsRemaining = _focusService.GetRepetitionsRemaining(e);
        SecondsRingVisible = true;
        SecondsRemaining = e.Remaining.Seconds;
        UpdateButtonStates();

        if (SkipSegmentRequested)
        {
            SkipSegmentRequested = false;
        }
    }
}
