using AmbientSounds.Constants;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using JeniusApps.Common.Telemetry;

namespace AmbientSounds.Services
{
    public class AppCenterTelemetry : ITelemetry
    {
        private readonly IUserSettings _userSettings;

        public AppCenterTelemetry(
            IUserSettings userSettings,
            IAppSettings appSettings)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _userSettings = userSettings;

            AppCenter.Start(appSettings.TelemetryApiKey, typeof(Analytics), typeof(Crashes));
        }

        /// <inheritdoc/>
        public void TrackError(Exception e, IDictionary<string, string>? properties = null)
        {
            Crashes.TrackError(e, properties);
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null)
        {
            if (_userSettings.Get<bool>(UserSettingsConstants.TelemetryOn))
            {
                Analytics.TrackEvent(eventName, properties);
            }
        }
    }
}
