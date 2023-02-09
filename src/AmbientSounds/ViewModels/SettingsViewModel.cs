using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private const string NoneImageName = "znone.png";
        private readonly IImagePicker _imagePicker;
        private readonly IAssetsReader _assetsReader;
        private readonly IUserSettings _userSettings;
        private readonly IStoreNotificationRegistrar _notifications;
        private readonly ITelemetry _telemetry;
        private readonly IAppStoreRatings _appStoreRatings;
        private bool _notificationsLoading;

        public SettingsViewModel(
            IUserSettings userSettings,
            IStoreNotificationRegistrar notifications,
            ITelemetry telemetry,
            IAssetsReader assetsReader,
            IImagePicker imagePicker,
            IAppStoreRatings appStoreRatings)
        {
            Guard.IsNotNull(userSettings);
            Guard.IsNotNull(notifications);
            Guard.IsNotNull(telemetry);
            Guard.IsNotNull(assetsReader);
            Guard.IsNotNull(imagePicker);
            Guard.IsNotNull(appStoreRatings);
            _userSettings = userSettings;
            _notifications = notifications;
            _telemetry = telemetry;
            _assetsReader = assetsReader;
            _imagePicker = imagePicker;
            _appStoreRatings = appStoreRatings;
        }

        /// <summary>
        /// Paths to available background images.
        /// </summary>
        public ObservableCollection<string> ImagePaths { get; } = new();

        /// <summary>
        /// The current theme.
        /// </summary>
        public string CurrentTheme
        {
            get => _userSettings.Get<string>(UserSettingsConstants.Theme);
            set
            {
                _userSettings.Set(UserSettingsConstants.Theme, value);
                OnPropertyChanged(nameof(CurrentThemeIndex));
            }
        }

        public int CurrentThemeIndex => CurrentTheme switch
        {
            "default" => 0,
            "dark" => 1,
            "light" => 2,
            _ => 0
        };

        /// <summary>
        /// Settings flag for telemetry.
        /// </summary>
        public bool TelemetryEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.TelemetryOn);
            set => _userSettings.Set(UserSettingsConstants.TelemetryOn, value);
        }

        /// <summary>
        /// Settings flag for resume on launch.
        /// </summary>
        public bool ResumeOnLaunchEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.ResumeOnLaunchKey);
            set => _userSettings.Set(UserSettingsConstants.ResumeOnLaunchKey, value);
        }

        /// <summary>
        /// Settings flag for resume on launch.
        /// </summary>
        public bool AmbieMiniEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.CompactOnFocusKey);
            set => _userSettings.Set(UserSettingsConstants.CompactOnFocusKey, value);
        }

        /// <summary>
        /// Settings flag for resume on launch.
        /// </summary>
        public bool SmtcDisabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.DisableSmtcSupportKey);
            set
            {
                _userSettings.Set(UserSettingsConstants.DisableSmtcSupportKey, value);
                _telemetry.TrackEvent(value
                    ? TelemetryConstants.SmtcDisabled
                    : TelemetryConstants.SmtcEnabled);
            }
        }

        /// <summary>
        /// Settings flag for notifications.
        /// </summary>
        public bool NotificationsEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.Notifications);
            set => SetNotifications(value);
        }

        private async void SetNotifications(bool value)
        {
            if (value == NotificationsEnabled || _notificationsLoading)
            {
                return;
            }

            _notificationsLoading = true;
            if (value)
            {
                await _notifications.Register();
            }
            else
            {
                await _notifications.Unregiser();
            }
            _userSettings.Set(UserSettingsConstants.Notifications, value);
            _notificationsLoading = false;
        }

        public void UpdateTheme(string newTheme)
        {
            if (newTheme is "default" or "dark" or "light")
            {
                CurrentTheme = newTheme;
            }
        }

        [RelayCommand]
        private async Task LoadImagesAsync()
        {
            if (ImagePaths.Count > 0)
            {
                return;
            }

            var paths = await _assetsReader.GetBackgroundsAsync();
            foreach (var p in paths)
            {
                ImagePaths.Add(p);
            }
        }

        [RelayCommand]
        private async Task BrowseAsync()
        {
            string? imagePath = await _imagePicker.BrowseAsync();
            if (imagePath == null)
            {
                return;
            }

            SelectImage(imagePath);
        }

        [RelayCommand]
        private void SelectImage(string? imagePath)
        {
            if (imagePath?.Contains(NoneImageName) == true)
            {
                imagePath = string.Empty;
            }

            _userSettings.Set(UserSettingsConstants.BackgroundImage, imagePath);

            if (imagePath != null)
            {
                _telemetry.TrackEvent(TelemetryConstants.BackgroundChanged, new Dictionary<string, string>
                {
                    { "path", imagePath.Contains("ms-appx") ? imagePath : "custom" }
                });
            }
        }

        [RelayCommand]
        private async Task RequestRatingAsync()
        {
            bool result = await _appStoreRatings.RequestInAppRatingsAsync();
            if (result)
            {
                _userSettings.Set(UserSettingsConstants.HasRated, true);
            }
        }
    }
}