﻿using AmbientSounds.Constants;
using AmbientSounds.Extensions;
using AmbientSounds.Models;
using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels
{
    public partial class FocusTimerModuleViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
        private readonly IUserSettings _userSettings;
        private readonly ILocalizer _localizer;
        private readonly ITelemetry _telemetry;
        private readonly IRecentFocusService _recentFocusService;
        private readonly IFocusHistoryService _focusHistoryService;
        private readonly IFocusTaskService _taskService;
        private bool _isHelpMessageVisible;
        private int _focusLength;
        private int _restLength;
        private int _repetitions;

        [ObservableProperty]
        private bool _secondsRingVisible;
        [ObservableProperty]
        private int _secondsRemaining;
        [ObservableProperty]
        private int _focusLengthRemaining;
        [ObservableProperty]
        private int _restLengthRemaining;
        [ObservableProperty]
        private int _repetitionsRemaining;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsHelpIconVisible))]
        private bool _slidersEnabled;
        [ObservableProperty]
        private bool _playEnabled;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TasksVisible))]
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
        private string _currentStatus = string.Empty;
        [ObservableProperty]
        private int _selectedTaskIndex;

        public FocusTimerModuleViewModel(
            IFocusService focusService,
            ILocalizer localizer,
            IRecentFocusService recentFocusService,
            ITelemetry telemetry,
            IFocusHistoryService focusHistoryService,
            IUserSettings userSettings,
            IFocusTaskService taskService)
        {
            Guard.IsNotNull(focusService);
            Guard.IsNotNull(userSettings);
            Guard.IsNotNull(localizer);
            Guard.IsNotNull(telemetry);
            Guard.IsNotNull(recentFocusService);
            Guard.IsNotNull(focusHistoryService);
            Guard.IsNotNull(taskService);
            _focusService = focusService;
            _userSettings = userSettings;
            _localizer = localizer;
            _telemetry = telemetry;
            _recentFocusService = recentFocusService;
            _focusHistoryService = focusHistoryService;
            _taskService = taskService;
            IsHelpMessageVisible = !userSettings.Get<bool>(UserSettingsConstants.HasClosedFocusHelpMessageKey);
            UpdateButtonStates();
            InterruptionCommand = new AsyncRelayCommand(LogInterruptionAsync);
        }

        public ObservableCollection<RecentFocusSettings> RecentSettings { get; } = new();

        public ObservableCollection<FocusTaskViewModel> FocusTasks { get; } = new();

        public bool TasksVisible => CancelVisible && 
            FocusTasks.Count > 0 && 
            _focusService.CurrentSessionType == SessionType.Focus;

        public bool CountdownVisible => CancelVisible &&
            (FocusTasks.Count == 0 || _focusService.CurrentSessionType == SessionType.Rest);

        public bool IsRecentVisible => _focusService.CurrentState == FocusState.None && RecentSettings.Count > 0;

        public IAsyncRelayCommand InterruptionCommand { get; }

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

        public int FocusLength
        {
            get => _focusLength;
            set
            {
                SetProperty(ref _focusLength, value);
                OnPropertyChanged(nameof(TotalTime));
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

        public async Task InitializeAsync()
        {
            _focusService.TimeUpdated += OnTimeUpdated;
            _focusService.FocusStateChanged += OnFocusStateChanged;

            await InitializeTasksAsync();

            // Initialize recent list
            if (RecentSettings.Count > 0)
            {
                RecentSettings.Clear();
            }

            var recents = await _recentFocusService.GetRecentAsync();
            foreach (var recent in recents.OrderByDescending(x => x.LastUsed))
            {
                RecentSettings.Add(recent);
            }

            OnPropertyChanged(nameof(IsRecentVisible));
        }

        public void Uninitialize()
        {
            _focusService.TimeUpdated -= OnTimeUpdated;
            _focusService.FocusStateChanged -= OnFocusStateChanged;

            RecentSettings.Clear();
            FocusTasks.Clear();
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
            _telemetry.TrackEvent(TelemetryConstants.FocusPaused);
        }

        public void Stop()
        {
            _focusService.StopTimer();
            _telemetry.TrackEvent(TelemetryConstants.FocusReset);
        }

        public void ShowHelpMessage()
        {
            IsHelpMessageVisible = true;
            _telemetry.TrackEvent(TelemetryConstants.FocusHelpClicked);
        }

        public void LoadRecentSettings(RecentFocusSettings settings)
        {
            if (settings is null)
            {
                return;
            }

            FocusLength = settings.FocusMinutes;
            RestLength = settings.RestMinutes;
            Repetitions = settings.Repeats;


            var index = RecentSettings.IndexOf(settings);
            _telemetry.TrackEvent(TelemetryConstants.FocusRecentClicked, new Dictionary<string, string>
            {
                { "index", index.ToString() }
            });
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
            if (SelectedTaskIndex < FocusTasks.Count - 1)
            {
                SelectedTaskIndex += 1;
            }

            _telemetry.TrackEvent(TelemetryConstants.TaskCompletedInSession);
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
            _telemetry.TrackEvent(TelemetryConstants.TaskReopenedInSession);
        }

        private void OnFocusStateChanged(object sender, FocusState e)
        {
            OnPropertyChanged(nameof(IsRecentVisible));
            UpdateButtonStates();
        }

        private async void Start()
        {
            bool successfullyStarted;

            if (_focusService.CurrentState == FocusState.Paused)
            {
                successfullyStarted = _focusService.ResumeTimer();
                _telemetry.TrackEvent(TelemetryConstants.FocusResumed);
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
            }

            if (successfullyStarted)
            {
                _ = _recentFocusService.AddRecentAsync(FocusLength, RestLength, Repetitions);

                // Note that if the timer
                // didn't start successfully, it does not mean
                // we should hide the seconds ring. We don't care what the current state
                // is. All we care is that if it was started,
                // make sure the ring is visible.
                SecondsRingVisible = true;
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
            OnPropertyChanged(nameof(CountdownVisible));
        }

        private async Task LogInterruptionAsync()
        {
            (double minutesLogged, bool hasNotes) = await _focusHistoryService.LogInterruptionAsync();

            if (minutesLogged > 0)
            {
                _telemetry.TrackEvent(TelemetryConstants.FocusInterruptionLogged, new Dictionary<string, string>
                {
                    { "minutes", minutesLogged.ToString() },
                    { "hasNotes", hasNotes.ToString() }
                });
            }
        }

        private void OnTimeUpdated(object sender, FocusSession e)
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
                CurrentStatus = string.Empty;
                UpdateButtonStates();

                return;
            }

            CurrentTimeRemaining = e.Remaining.ToCountdownFormat();
            CurrentStatus = e.SessionType.ToDisplayString(_localizer);

            if (e.SessionType == SessionType.Focus)
            {
                FocusLengthRemaining = e.Remaining.Minutes;
                RestLengthRemaining = RestLength;
            }
            else if (e.SessionType == SessionType.Rest)
            {
                FocusLengthRemaining = 0;
                RestLengthRemaining = e.Remaining.Minutes;
            }

            RepetitionsRemaining = _focusService.GetRepetitionsRemaining(e);
            SecondsRingVisible = true;
            SecondsRemaining = e.Remaining.Seconds;
            UpdateButtonStates();
        }
    }
}
