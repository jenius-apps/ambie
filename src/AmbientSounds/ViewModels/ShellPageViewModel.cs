using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// ViewModel for the shell page.
    /// </summary>
    public class ShellPageViewModel : ObservableObject
    {
        private readonly IUserSettings _userSettings;
        private readonly ITimerService _ratingTimer;
        private readonly ITelemetry _telemetry;
        private bool _isRatingMessageVisible;

        public ShellPageViewModel(
            IUserSettings userSettings,
            ITimerService timer,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(timer, nameof(timer));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _userSettings = userSettings;
            _ratingTimer = timer;
            _telemetry = telemetry;

            _userSettings.SettingSet += OnSettingSet;

            if (!_userSettings.Get<bool>(UserSettingsConstants.HasRated))
            {
                _ratingTimer.Interval = 1800000; // 30 minutes
                _ratingTimer.IntervalElapsed += OnIntervalLapsed;
                _ratingTimer.Start();
            }
        }

        /// <summary>
        /// Determines if the rating message is visible.
        /// </summary>
        public bool IsRatingMessageVisible
        {
            get => _isRatingMessageVisible;
            set => SetProperty(ref _isRatingMessageVisible, value);
        }

        /// <summary>
        /// Path to background image.
        /// </summary>
        public string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage);

        /// <summary>
        /// Determines if the background image should be shown.
        /// </summary>
        public bool ShowBackgroundImage => !string.IsNullOrWhiteSpace(BackgroundImagePath);

        public void Dispose()
        {
            _userSettings.SettingSet -= OnSettingSet;
        }

        private void OnIntervalLapsed(object sender, int e)
        {
            _ratingTimer.Stop();
            _ratingTimer.IntervalElapsed -= OnIntervalLapsed;
            IsRatingMessageVisible = true;
            _telemetry.TrackEvent(TelemetryConstants.RatingMessageShown);
        }

        private void OnSettingSet(object sender, string settingsKey)
        {
            if (settingsKey == UserSettingsConstants.BackgroundImage)
            {
                OnPropertyChanged(nameof(ShowBackgroundImage));
                OnPropertyChanged(nameof(BackgroundImagePath));
            }
        }
    }
}
