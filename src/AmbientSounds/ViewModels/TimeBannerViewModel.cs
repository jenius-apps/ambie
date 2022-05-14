using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Diagnostics;
using AmbientSounds.Services;
using System;
using System.Collections.Generic;
using System.Text;
using JeniusApps.Common.Tools;
using AmbientSounds.Extensions;

namespace AmbientSounds.ViewModels
{
    public class TimeBannerViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
        private readonly ILocalizer _localizer;
        private string _statusText = string.Empty;
        private string _timeText = string.Empty;

        public TimeBannerViewModel(
            IFocusService focusService,
            ILocalizer localizer)
        {
            Guard.IsNotNull(focusService, nameof(focusService));
            Guard.IsNotNull(localizer, nameof(localizer));
            _focusService = focusService;
            _localizer = localizer;

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
            TimeText = e.Remaining.ToCountdownFormat();
            StatusText = e.SessionType.ToDisplayString(_localizer);
        }
    }
}
