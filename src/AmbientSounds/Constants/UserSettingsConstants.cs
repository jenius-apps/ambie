using System.Collections.Generic;

namespace AmbientSounds.Constants
{
    /// <summary>
    /// Class of key constants
    /// for user settings.
    /// </summary>
    public class UserSettingsConstants
    {
        /// <summary>
        /// Volume settings key.
        /// </summary>
        public const string Volume = "LastUsedVolume";

        /// <summary>
        /// Telemetry enabled key.
        /// </summary>
        public const string TelemetryOn = "TelemetryOn";

        /// <summary>
        /// Application theme settings key.
        /// </summary>
        public const string Theme = "themeSetting";

        /// <summary>
        /// Settings key for notifications.
        /// </summary>
        public const string Notifications = "NotificationSetting";

        /// <summary>
        /// If true, screen saver will be triggered automatically.
        /// </summary>
        public const string EnableScreenSaver = "EnableScreenSaver";

        /// <summary>
        /// If true, the screensaver will just be a dark, blank page.
        /// </summary>
        public const string DarkScreensasver = "DarkScreensaver";

        /// <summary>
        ///  Settings defaults.
        /// </summary>
        public static readonly Dictionary<string, object> Defaults = new Dictionary<string, object>()
        {
            { Volume, 80d },
            { TelemetryOn, true },
            { Notifications, true },
            { EnableScreenSaver, false },
            { DarkScreensasver, false },
            { Theme, "default" }
        };
    }
}
