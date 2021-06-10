using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace AmbientSounds.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly IUserSettings _userSettings;
        private readonly IStoreNotificationRegistrar _notifications;
        private readonly IScreensaverService _screensaverService;
        private readonly ISystemInfoProvider _systemInfoProvider;
        private readonly ITelemetry _telemetry;
        private bool _notificationsLoading;

        public SettingsViewModel(
            IUserSettings userSettings,
            IScreensaverService screensaverService,
            ISystemInfoProvider systemInfoProvider,
            IStoreNotificationRegistrar notifications,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(notifications, nameof(notifications));
            Guard.IsNotNull(screensaverService, nameof(screensaverService));
            Guard.IsNotNull(systemInfoProvider, nameof(systemInfoProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            _systemInfoProvider = systemInfoProvider;
            _screensaverService = screensaverService;
            _userSettings = userSettings;
            _notifications = notifications;
            _telemetry = telemetry;
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
    }
}