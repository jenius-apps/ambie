using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class SettingsViewModel
    {
        private readonly IUserSettings _userSettings;
        private readonly IStoreNotificationRegistrar _notifications;
        private readonly IScreensaverService _screensaverService;
        private readonly ISystemInfoProvider _systemInfoProvider;
        private readonly string _theme;
        private bool _notificationsLoading;

        public SettingsViewModel(
            IUserSettings userSettings,
            IScreensaverService screensaverService,
            ISystemInfoProvider systemInfoProvider,
            IStoreNotificationRegistrar notifications)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(notifications, nameof(notifications));
            Guard.IsNotNull(screensaverService, nameof(screensaverService));
            Guard.IsNotNull(systemInfoProvider, nameof(systemInfoProvider));
            _systemInfoProvider = systemInfoProvider;
            _screensaverService = screensaverService;
            _userSettings = userSettings;
            _notifications = notifications;
            _theme = _userSettings.Get<string>(UserSettingsConstants.Theme);
            InitializeTheme();

            SelectImageCommand = new RelayCommand<string>(SelectImage);
            LoadImagesCommand = new AsyncRelayCommand(LoadImagesAsync);
        }

        /// <summary>
        /// Command for selecting the background image.
        /// </summary>
        public IRelayCommand<string> SelectImageCommand { get; }

        /// <summary>
        /// Command for loading the background images.
        /// </summary>
        public IAsyncRelayCommand LoadImagesCommand { get; }

        /// <summary>
        /// Paths to available background images.
        /// </summary>
        public ObservableCollection<string> ImagePaths { get; } = new();

        /// <summary>
        /// Settings flag for telemetry.
        /// </summary>
        public bool TelemetryEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.TelemetryOn);
            set => _userSettings.Set(UserSettingsConstants.TelemetryOn, value);
        }

        /// <summary>
        /// Settings flag for performance mode.
        /// </summary>
        public bool PerformanceModeEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.PerformanceMode);
            set => _userSettings.Set(UserSettingsConstants.PerformanceMode, value);
        }

        /// <summary>
        /// Settings flag for screensaver.
        /// </summary>
        public bool ScreensaverEnabled
        {
            get => _userSettings.Get(UserSettingsConstants.EnableScreenSaver, _systemInfoProvider.IsTenFoot());
            set
            {
                _userSettings.Set(UserSettingsConstants.EnableScreenSaver, value);
                
                if (value)
                {
                    _screensaverService.StartTimer();
                }
                else
                {
                    _screensaverService.StopTimer();
                }
            }
        }

        /// <summary>
        /// Settings flag for dark screensaver.
        /// </summary>
        public bool DarkScreensaverEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.DarkScreensasver);
            set => _userSettings.Set(UserSettingsConstants.DarkScreensasver, value);
        }

        /// <summary>
        /// Settings flag for notifications.
        /// </summary>
        public bool NotificationsEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.Notifications);
            set => SetNotifications(value);
        }

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for default app theme.
        /// </summary>
        public bool DefaultRadioIsChecked { get; set; }

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for dark app theme.
        /// </summary>
        public bool DarkRadioIsChecked { get; set; }

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for light app theme.
        /// </summary>
        public bool LightRadioIsChecked { get; set; }

        /// <summary>
        /// Event handler for RadioButton (dark theme) click event.
        /// </summary>
        public void DarkThemeRadioClicked()
        {
            _userSettings.Set(UserSettingsConstants.Theme, "dark");
        }

        /// <summary>
        /// Event handler for RadioButton (default theme) click event.
        /// </summary>
        public void DefaultThemeRadioClicked()
        {
            _userSettings.Set(UserSettingsConstants.Theme, "default");
        }

        /// <summary>
        /// Event handler for RadioButton (light theme) click event.
        /// </summary>
        public void LightThemeRadioClicked()
        {
            _userSettings.Set(UserSettingsConstants.Theme, "light");
        }

        private void SelectImage(string? imagePath)
        {
            _userSettings.Set(UserSettingsConstants.BackgroundImage, imagePath);
        }

        private async Task LoadImagesAsync()
        {
            if (ImagePaths.Count > 0)
            {
                return;
            }

            string[] paths = await _systemInfoProvider.GetAvailableBackgroundsAsync();
            foreach (var p in paths)
            {
                ImagePaths.Add(p);
            }
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

        /// <summary>
        /// Method for setting appropriate radio button on for each app theme.
        /// </summary>
        private void InitializeTheme()
        {
            if (_theme != null)
            {
                switch (_theme)
                {
                    case "default":
                        DefaultRadioIsChecked = true;
                        break;
                    case "light":
                        LightRadioIsChecked = true;
                        break;
                    case "dark":
                        DarkRadioIsChecked = true;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}