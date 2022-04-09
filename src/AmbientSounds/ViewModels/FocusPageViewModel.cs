using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class FocusPageViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
        private int _focusLength;
        private int _restLength;
        private int _repetitions;
        private int _focusLengthRemaining;
        private int _restLengthRemaining;
        private int _repetitionsRemaining;
        private string _currentTimeRemaining = string.Empty;
        private string _currentStatus = string.Empty;

        public FocusPageViewModel(IFocusService focusService)
        {
            Guard.IsNotNull(focusService, nameof(focusService));
            _focusService = focusService;
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
    }
}
