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
        ///  Settings defaults.
        /// </summary>
        public static readonly Dictionary<string, object> Defaults = new Dictionary<string, object>()
        {
            { Volume, 80d },
            { TelemetryOn, true }
        };
    }
}
