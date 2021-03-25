using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using System.Collections.Generic;

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
        private readonly string _preferredLanguage;

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
        }

        /// <summary>
        /// Settings flag for telemetry.
        /// </summary>
        public bool TelemetryEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.TelemetryOn);
            set => _userSettings.Set(UserSettingsConstants.TelemetryOn, value);
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
        /// Settings flag for language override.
        /// </summary>
        public bool OverrideLanguage
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.OverrideLanguage);
            set => _userSettings.Set(UserSettingsConstants.OverrideLanguage, value);
        }

        /// <summary>
        /// Settings flag for preferred language.
        /// </summary>
        public string PreferredLanguage
        {
            get => _userSettings.Get<string>(UserSettingsConstants.PreferredLanguage);
            set => _userSettings.Set(UserSettingsConstants.PreferredLanguage, value);
        }


        public List<string> AvailableLanugages { get; set; } = new List<string>();//code behind will add common (OS and APP) languages here.



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