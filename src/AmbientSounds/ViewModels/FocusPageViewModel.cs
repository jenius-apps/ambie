using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace AmbientSounds.ViewModels
{
    public class FocusPageViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
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

        public FocusPageViewModel(IFocusService focusService)
        {
            Guard.IsNotNull(focusService, nameof(focusService));
            _focusService = focusService;

            _focusService.TimeUpdated += OnTimeUpdated;
            _focusService.FocusStateChanged += OnFocusStateChanged;
        }

        public int FocusLength
        {
            get => _focusLength;
            set 
            { 
                SetProperty(ref _focusLength, value);
                OnPropertyChanged(nameof(TotalTime));
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

        public bool SlidersEnabled => _focusService.CurrentState == FocusState.None;

        public bool PlayVisible => _focusService.CurrentState == FocusState.Paused || _focusService.CurrentState == FocusState.None;

        public bool PauseVisible => _focusService.CurrentState == FocusState.Active;

        public bool CancelVisible => _focusService.CurrentState == FocusState.Active || _focusService.CurrentState == FocusState.Paused;

        public void Start()
        {
            bool successfullyStarted;

            if (_focusService.CurrentState == FocusState.Paused)
            {
                successfullyStarted = _focusService.ResumeTimer();
            }
            else
            {
                SecondsRemaining = 60;
                successfullyStarted = _focusService.StartTimer(FocusLength, RestLength, Repetitions);
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

        public void Pause()
        {
            _focusService.PauseTimer();
        }

        public void Stop()
        {
            _focusService.StopTimer();
        }

        private void UpdateButtonVisibilities()
        {
            OnPropertyChanged(nameof(SlidersEnabled));
            OnPropertyChanged(nameof(PlayVisible));
            OnPropertyChanged(nameof(PauseVisible));
            OnPropertyChanged(nameof(CancelVisible));
        }

        private void OnFocusStateChanged(object sender, FocusState e)
        {
            UpdateButtonVisibilities();
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
                UpdateButtonVisibilities();

                return;
            }

            CurrentTimeRemaining = e.Remaining.ToString(@"mm\:ss");
            CurrentStatus = e.SessionType.ToString();

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
            UpdateButtonVisibilities();
        }
    }
}
