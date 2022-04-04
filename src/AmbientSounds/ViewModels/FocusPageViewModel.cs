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
            }
        }

        public int RestLength
        {
            get => _restLength;
            set
            {
                SetProperty(ref _restLength, value);
                OnPropertyChanged(nameof(TotalTime));
            }
        }

        public int Repetitions
        {
            get => _repetitions;
            set
            {
                SetProperty(ref _repetitions, value);
                OnPropertyChanged(nameof(TotalTime));
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
    }
}
