using AmbientSounds.Constants;
using AmbientSounds.Extensions;
using AmbientSounds.Models;
using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class FocusTimerModuleViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
        private readonly IUserSettings _userSettings;
        private readonly ILocalizer _localizer;
        private readonly ITelemetry _telemetry;
        private readonly IRecentFocusService _recentFocusService;
        private readonly IFocusHistoryService _focusHistoryService;
        private bool _secondsRingVisible;
        private int _secondsRemaining;
        private int _focusLengthRemaining;
        private int _restLengthRemaining;
        private int _repetitionsRemaining;
        private bool _slidersEnabled;
        private bool _isHelpMessageVisible;
        private int _focusLength;
        private int _restLength;
        private int _repetitions;
        private bool _playEnabled;
        private bool _cancelVisible;
        private bool _playVisible;
        private bool _pauseVisible;
        private string _primaryButtonText = string.Empty;
        private string _currentTimeRemaining = string.Empty;
        private string _currentStatus = string.Empty;

        public FocusTimerModuleViewModel(
            IFocusService focusService,
            ILocalizer localizer,
            IRecentFocusService recentFocusService,
            ITelemetry telemetry,
            IFocusHistoryService focusHistoryService,
            IUserSettings userSettings)
        {
            Guard.IsNotNull(focusService, nameof(focusService));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(localizer, nameof(localizer));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(recentFocusService, nameof(recentFocusService));
            Guard.IsNotNull(focusHistoryService, nameof(focusHistoryService));
            _focusService = focusService;
            _userSettings = userSettings;
            _localizer = localizer;
            _telemetry = telemetry;
            _recentFocusService = recentFocusService;
            _focusHistoryService = focusHistoryService;
            IsHelpMessageVisible = !userSettings.Get<bool>(UserSettingsConstants.HasClosedFocusHelpMessageKey);
            UpdateButtonStates();
            InterruptionCommand = new AsyncRelayCommand(LogInterruptionAsync);
        }

        public ObservableCollection<RecentFocusSettings> RecentSettings { get; } = new();

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

        public bool SecondsRingVisible
        {
            get => _secondsRingVisible;
            set => SetProperty(ref _secondsRingVisible, value);
        }

        public int SecondsRemaining
        {
            get => _secondsRemaining;
            set => SetProperty(ref _secondsRemaining, value);
        }

        public int FocusLengthRemaining
        {
            get => _focusLengthRemaining;
            set => SetProperty(ref _focusLengthRemaining, value);
        }

        public int RestLengthRemaining
        {
            get => _restLengthRemaining;
            set => SetProperty(ref _restLengthRemaining, value);
        }

        public int RepetitionsRemaining
        {
            get => _repetitionsRemaining;
            set => SetProperty(ref _repetitionsRemaining, value);
        }

        public bool SlidersEnabled
        {
            get => _slidersEnabled;
            set
            {
                if (SetProperty(ref _slidersEnabled, value))
                {
                    OnPropertyChanged(nameof(IsHelpIconVisible));
                }
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

        public bool PlayEnabled
        {
            get => _playEnabled;
            set => SetProperty(ref _playEnabled, value);
        }

        public bool PlayVisible
        {
            get => _playVisible;
            set => SetProperty(ref _playVisible, value);
        }

        public bool PauseVisible
        {
            get => _pauseVisible;
            set => SetProperty(ref _pauseVisible, value);
        }

        public bool CancelVisible
        {
            get => _cancelVisible;
            set => SetProperty(ref _cancelVisible, value);
        }

        public string PrimaryButtonText
        {
            get => _primaryButtonText;
            set => SetProperty(ref _primaryButtonText, value);
        }

        public string CurrentTimeRemaining
        {
            get => _currentTimeRemaining;
            set => SetProperty(ref _currentTimeRemaining, value);
        }

        public string CurrentStatus
        {
            get => _currentStatus;
            set => SetProperty(ref _currentStatus, value);
        }

        public async Task InitializeAsync()
        {
            _focusService.TimeUpdated += OnTimeUpdated;
            _focusService.FocusStateChanged += OnFocusStateChanged;

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

        private void OnFocusStateChanged(object sender, FocusState e)
        {
            OnPropertyChanged(nameof(IsRecentVisible));
            UpdateButtonStates();
        }

        private void Start()
        {
            bool successfullyStarted;

            if (_focusService.CurrentState == FocusState.Paused)
            {
                successfullyStarted = _focusService.ResumeTimer();
                _telemetry.TrackEvent(TelemetryConstants.FocusResumed);
            }
            else
            {
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
