using AmbientSounds.Constants;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace AmbientSounds.Services.Uwp
{
    public class ScreensaverService : IScreensaverService
    {
        private readonly IUserSettings _settings;
        private readonly ITelemetry _telemetry;
        private readonly INavigator _navigator;
        private DispatcherTimer _screensaverTriggerTimer;

        public ScreensaverService(
            ITelemetry telemetry,
            IUserSettings userSettings,
            INavigator navigator)
        {
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(navigator, nameof(navigator));
            _telemetry = telemetry;
            _settings = userSettings;
            _navigator = navigator;
        }

        /// <inheritdoc/>
        public bool IsScreensaverEnabled
        {
            get => _settings.Get(UserSettingsConstants.EnableScreenSaver, App.IsTenFoot);
        }

        /// <inheritdoc/>
        public void StartTimer()
        {
            if (!IsScreensaverEnabled || _screensaverTriggerTimer?.IsEnabled == true)
            {
                return;
            }

            _screensaverTriggerTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
            _screensaverTriggerTimer.Tick += ScreensaverTriggered;
            _screensaverTriggerTimer.Start();
        }

        /// <inheritdoc/>
        public void StopTimer()
        {
            if (_screensaverTriggerTimer != null)
            {
                _screensaverTriggerTimer.Stop();
                _screensaverTriggerTimer.Tick -= ScreensaverTriggered;
                _screensaverTriggerTimer = null;
            }
        }

        /// <inheritdoc/>
        public void ResetScreensaverTimeout()
        {
            if (_screensaverTriggerTimer != null)
            {
                _screensaverTriggerTimer.Stop();
                _screensaverTriggerTimer.Start();
            }
        }

        private void ScreensaverTriggered(object sender, object e)
        {
            _telemetry.TrackEvent(TelemetryConstants.ScreensaverOpened, new Dictionary<string, string>()
            {
                { "trigger", "timer" }
            });
            _navigator.ToScreensaver();
        }
    }
}
