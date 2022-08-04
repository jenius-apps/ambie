using AmbientSounds.Models;
using System;
using System.Collections.Generic;

namespace AmbientSounds.Constants
{
    /// <summary>
    /// Class of key constants for user settings.
    /// </summary>
    public static class UserSettingsConstants
    {
        /// <summary>
        /// Key to remember if the user has rated the app
        /// for this installation.
        /// </summary>
        public const string HasRated = "HasRated";

        /// <summary>
        /// Key to remember recent focus settings.
        /// </summary>
        public const string RecentFocusKey = "RecentFocusSettings";

        /// <summary>
        /// Key to remember the UTC date that the user dismissed the rating prompt.
        /// </summary>
        public const string RatingDismissed = "RatingDismissed";

        /// <summary>
        /// Key to remember the user's background image.
        /// </summary>
        public const string BackgroundImage = "BackgroundImagePath";

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
        /// The number of max active tracks.
        /// </summary>
        public const string MaxActive = "MaxActive";

        /// <summary>
        /// Key for the list of active tracks.
        /// </summary>
        public const string ActiveTracks = "ActiveTracks";

        /// <summary>
        /// Key for the active mix Id.
        /// </summary>
        public const string ActiveMixId = "ActiveMixId";

        /// <summary>
        /// Key used to fetch the stored auth provider Id. This is used for signing into the MSA account silently.
        /// </summary>
        public const string CurrentUserProviderId = "CurrentUserProviderId";

        /// <summary>
        /// Key used to fetch the stored user Id. This is used for signing into the MSA account silently.
        /// </summary>
        public const string CurrentUserId = "CurrentUserId";

        /// <summary>
        /// Used to remember if Ambie should resume playing sounds immediately
        /// after launching.
        /// </summary>
        public const string ResumeOnLaunchKey = "ResumeOnLaunch";

        /// <summary>
        /// User to remember if Ambie should integrate with
        /// OS media controls
        /// </summary>
        public const string DisableSmtcSupportKey = "DisableSmtcSupportKey";

        /// <summary>
        /// Used to remember what was the last used screensaver ID.
        /// </summary>
        public const string LastUsedScreensaverKey = "LastUsedScreensaver";

        /// <summary>
        /// Used to remember if the user has closed the focus help message
        /// at least once.
        /// </summary>
        public const string HasClosedFocusHelpMessageKey = "HasClosedFocusHelpMessage";

        /// <summary>
        /// Used to remember if the user has closed the interruption help
        /// at least once.
        /// </summary>
        public const string HasClosedInterruptionMessageKey = "HasClosedInterruptionMessage";

        /// <summary>
        /// Used to remember a unique device ID for the purposes of tracking presence counts.
        /// </summary>
        public const string DevicePresenceIdKey = "DevicePresenceId";

        /// <summary>
        ///  Settings defaults.
        /// </summary>
        public static IReadOnlyDictionary<string, object> Defaults { get; } = new Dictionary<string, object>()
        {
            { Volume, 80d },
            { TelemetryOn, true },
            { Notifications, true },
            { MaxActive, 3 },
            { ActiveTracks, Array.Empty<string>() },
            { ActiveMixId, string.Empty },
            { CurrentUserId, string.Empty },
            { CurrentUserProviderId, string.Empty },
            { Theme, "default" },
            { BackgroundImage, string.Empty },
            { HasRated, false },
            { RatingDismissed, DateTime.MinValue },
            { ResumeOnLaunchKey, false },
            { DisableSmtcSupportKey, false },
            { LastUsedScreensaverKey, string.Empty },
            { HasClosedFocusHelpMessageKey, false },
            { HasClosedInterruptionMessageKey, false },
            { RecentFocusKey, Array.Empty<RecentFocusSettings>() },
            { DevicePresenceIdKey, string.Empty }
        };
    }
}
