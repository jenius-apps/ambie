using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private readonly INavigator _navigator;
        private readonly IDialogService _dialogService;
        private readonly IIapService _iapService;
        private bool _isRatingMessageVisible;
        private bool _premiumButtonVisible;

        public ShellPageViewModel(
            IUserSettings userSettings,
            ITimerService timer,
            ITelemetry telemetry,
            ISystemInfoProvider systemInfoProvider,
            INavigator navigator,
            IDialogService dialogService,
            IIapService iapService)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(timer, nameof(timer));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(navigator, nameof(navigator));
            Guard.IsNotNull(dialogService, nameof(dialogService));
            Guard.IsNotNull(iapService, nameof(iapService));

            _userSettings = userSettings;
            _ratingTimer = timer;
            _telemetry = telemetry;
            _navigator = navigator;
            _dialogService = dialogService;
            _iapService = iapService;
            _iapService.ProductPurchased += OnProductPurchased;

            _userSettings.SettingSet += OnSettingSet;

            var lastDismissDateTime = _userSettings.GetAndDeserialize<DateTime>(UserSettingsConstants.RatingDismissed);
            if (!systemInfoProvider.IsFirstRun() &&
                !systemInfoProvider.IsTenFoot() &&
                !_userSettings.Get<bool>(UserSettingsConstants.HasRated) &&
                lastDismissDateTime.AddDays(7) <= DateTime.UtcNow)
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

        public bool PremiumButtonVisible
        {
            get => _premiumButtonVisible;
            set => SetProperty(ref _premiumButtonVisible, value);
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

        public void Navigate(int index)
        {
            if (index == 0)
            {
                _navigator.ToHome();
            }
            else if (index == 1)
            {
                _navigator.ToCatalogue();
            }
        }

        public async void OpenPremiumDialog()
        {
            _telemetry.TrackEvent(TelemetryConstants.ShellPagePremiumClicked);
            await _dialogService.OpenPremiumAsync();
        }

        public async void OpenThemeSettings() => await _dialogService.OpenThemeSettingsAsync();

        public void GoToScreensaver()
        {
            _telemetry.TrackEvent(TelemetryConstants.ScreensaverTriggered, new Dictionary<string, string>()
            {
                { "trigger", "mainPage" }
            });

            _navigator.ToScreensaver();
        }

        public async Task LoadPremiumButtonAsync()
        {
            PremiumButtonVisible = !await _iapService.IsOwnedAsync(IapConstants.MsStoreAmbiePlusId);
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

        private void OnProductPurchased(object sender, string iapId)
        {
            if (iapId == IapConstants.MsStoreAmbiePlusId)
            {
                PremiumButtonVisible = false;
            }
        }
    }
}
