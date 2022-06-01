using AmbientSounds.Constants;
using AmbientSounds.Extensions;
using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace AmbientSounds.ViewModels
{
    public class FocusPageViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
        private readonly ILocalizer _localizer;
        private readonly ITelemetry _telemetry;
        private readonly IUserSettings _userSettings;
        private int _focusLength;
        private int _restLength;
        private int _repetitions;
        private bool _secondsRingVisible;
        private int _secondsRemaining;
        private int _focusLengthRemaining;
        private int _restLengthRemaining;
        private int _repetitionsRemaining;
        private string _currentTimeRemaining = string.Empty;
        private string _currentStatus = string.Empty;
        private bool _playEnabled;
        private bool _slidersEnabled;
        private bool _playVisible;
        private bool _pauseVisible;
        private bool _cancelVisible;
        private string _primaryButtonText = string.Empty;
        private bool _isHelpMessageVisible;

        public FocusPageViewModel(
            IFocusService focusService,
            ILocalizer localizer,
            ITelemetry telemetry,
            IUserSettings userSettings)
        {
            Guard.IsNotNull(focusService, nameof(focusService));
            Guard.IsNotNull(localizer, nameof(localizer));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _focusService = focusService;
            _localizer = localizer;
            _telemetry = telemetry;
            _userSettings = userSettings;

            _focusService.TimeUpdated += OnTimeUpdated;
            _focusService.FocusStateChanged += OnFocusStateChanged;

            IsHelpMessageVisible = !userSettings.Get<bool>(UserSettingsConstants.HasClosedFocusHelpMessageKey);
            UpdateButtonStates();
        }

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
                TimeSpan time = _focusService.GetTotalTime(FocusLength, RestLength, Repetitions);
                return time.ToString(@"hh\:mm");
            }
        }

        public bool SecondsRingVisible
        {
            get => _secondsRingVisible;
            set
            {
                SetProperty(ref _secondsRingVisible, value);
            }
        }

        public int SecondsRemaining
        {
            get => _secondsRemaining;
            set
            {
                SetProperty(ref _secondsRemaining, value);
            }
        }


        public int FocusLengthRemaining
        {
            get => _focusLengthRemaining;
            set
            {
                SetProperty(ref _focusLengthRemaining, value);
            }
        }

        public int RestLengthRemaining
        {
            get => _restLengthRemaining;
            set
            {
                SetProperty(ref _restLengthRemaining, value);
            }
        }

        public int RepetitionsRemaining
        {
            get => _repetitionsRemaining;
            set
            {
                SetProperty(ref _repetitionsRemaining, value);
            }
        }

        public string CurrentStatus
        {
            get => _currentStatus;
            set
            {
                SetProperty(ref _currentStatus, value);
            }
        }

        public string CurrentTimeRemaining
        {
            get => _currentTimeRemaining;
            set
            {
                SetProperty(ref _currentTimeRemaining, value);
            }
        }

        public bool PlayEnabled
        {
            get => _playEnabled;
            set => SetProperty(ref _playEnabled, value);
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

        public bool IsHelpIconVisible => !IsHelpMessageVisible && SlidersEnabled;

        public bool TeachingTip1Visible { get; set; }
        public bool TeachingTip2Visible { get; set; }
        public bool TeachingTip3Visible { get; set; }
        public bool TeachingTip4Visible { get; set; }

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

        public void StartTutorial()
        {
            if (!SlidersEnabled)
            {
                return;
            }

            IsHelpMessageVisible = false;
            TeachingTip1Visible = true;
            OnPropertyChanged(nameof(TeachingTip1Visible));
        }

        public void ShowTip2()
        {
            TeachingTip1Visible = false;
            OnPropertyChanged(nameof(TeachingTip1Visible));

            if (!SlidersEnabled)
            {
                return;
            }

            TeachingTip2Visible = true;
            OnPropertyChanged(nameof(TeachingTip2Visible));
        }

        public void ShowTip3()
        {
            TeachingTip2Visible = false;
            OnPropertyChanged(nameof(TeachingTip2Visible));

            if (!SlidersEnabled)
            {
                return;
            }

            TeachingTip3Visible = true;
            OnPropertyChanged(nameof(TeachingTip3Visible));
        }

        public void ShowTip4()
        {
            TeachingTip3Visible = false;
            OnPropertyChanged(nameof(TeachingTip3Visible));

            if (!SlidersEnabled)
            {
                return;
            }

            TeachingTip4Visible = true;
            OnPropertyChanged(nameof(TeachingTip4Visible));
        }

        public void ShowHelpMessage()
        {
            IsHelpMessageVisible = true;
        }

        private void UpdatePrimaryButtonText()
        {
            PrimaryButtonText = PauseVisible
                ? _localizer.GetString("Pause")
                : _localizer.GetString("Start");
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
                    { "repetitions", Repetitions.ToString() }
                });
            }

            if (successfullyStarted)
            {
                // Note that if the timer
                // didn't start successfully, it does not mean
                // we should hide the seconds ring. We care what the current state
                // is. All we care is that if it was started,
                // make sure the ring is visible.
                SecondsRingVisible = true;
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
            UpdatePrimaryButtonText();
        }

        private void OnFocusStateChanged(object sender, FocusState e)
        {
            UpdateButtonStates();
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
