using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Diagnostics;
using AmbientSounds.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class TimeBannerViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
        private string _statusText = string.Empty;
        private string _timeText = string.Empty;

        public TimeBannerViewModel(
            IFocusService focusService)
        {
            Guard.IsNotNull(focusService, nameof(focusService));
            _focusService = focusService;

            _focusService.TimeUpdated += OnTimeUpdated;

        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                SetProperty(ref _statusText, value);
            }
        }

        public string TimeText
        {
            get => _timeText;
            set
            {
                SetProperty(ref _timeText, value);
            }
        }

        private void OnTimeUpdated(object sender, FocusSession e)
        {
            StatusText = e.SessionType.ToString();
            TimeText = e.Remaining.ToString(@"mm\:ss");
        }
    }
}
