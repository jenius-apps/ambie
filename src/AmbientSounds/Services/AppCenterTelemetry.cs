using AmbientSounds.Constants;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;

namespace AmbientSounds.Services
{
    public class AppCenterTelemetry : ITelemetry
    {
        private readonly IUserSettings _userSettings;

        public AppCenterTelemetry(
            IUserSettings userSettings,
            IAppSettings appSettings,
            ISystemInfoProvider systemInfoProvider)
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

        /// <inheritdoc/>
        public void SuggestSound(string soundSuggestion)
        {
            if (string.IsNullOrWhiteSpace(soundSuggestion))
                return;

            soundSuggestion = soundSuggestion.Trim().ToLower();

            Analytics.TrackEvent("soundSuggestion", new Dictionary<string, string>
            {
                { "value", soundSuggestion }
            });
        }
    }
}
