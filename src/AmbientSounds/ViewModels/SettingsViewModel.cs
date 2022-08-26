﻿using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbientSounds.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly IUserSettings _userSettings;
        private readonly IStoreNotificationRegistrar _notifications;
        private readonly ITelemetry _telemetry;
        private bool _notificationsLoading;

        public SettingsViewModel(
            IUserSettings userSettings,
            IStoreNotificationRegistrar notifications,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(notifications, nameof(notifications));
            Guard.IsNotNull(telemetry, nameof(telemetry));
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
    }
}