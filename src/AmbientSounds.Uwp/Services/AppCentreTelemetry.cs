using AmbientSounds.Constants;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using Windows.Globalization;

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Telemetry service for app centre.
    /// </summary>
    public class AppCentreTelemetry : ITelemetry
    {
        private readonly IUserSettings _userSettings;

        public AppCentreTelemetry(
            IUserSettings userSettings,
            IAppSettings appSettings)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _userSettings = userSettings;
            AppCenter.SetCountryCode(new GeographicRegion().CodeTwoLetter);
            AppCenter.Start(appSettings.TelemetryApiKey, typeof(Analytics), typeof(Crashes));
        }

        /// <inheritdoc/>
        public void TrackError(Exception e, IDictionary<string, string> properties = null)
        {
            Crashes.TrackError(e, properties);
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null)
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
