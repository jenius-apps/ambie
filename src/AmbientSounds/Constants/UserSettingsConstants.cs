using AmbientSounds.Models;
using System;
using System.Collections.Generic;

namespace AmbientSounds.Constants;

/// <summary>
/// Class of key constants for user settings.
/// </summary>
public static class UserSettingsConstants
{
    /// <summary>
    /// Unique ID assigned to the local user that persists across sessions.
    /// </summary>
    public const string LocalUserIdKey = "LocalUserId";

    /// <summary>
    /// Key to remember if the download message on
    /// home page should be shown or not.
    /// </summary>
    public const string ShowHomePageDownloadMessageKey = "ShowHomePageDownloadMessage";

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
    /// Used to remember if compact mode should be
    /// automatically opened when starting a focus session.
    /// </summary>
    public const string CompactOnFocusKey = "CompactOnFocus";

    /// <summary>
    /// Used to remember if packaged sounds have already been loaded,
    /// which usually occurs on first run of the app.
    /// </summary>
    public const string HasLoadedPackagedSoundsKey = "HasLoadedPackagedSounds";

    /// <summary>
    /// Used to remember if sounds should keep playing even after focus session ends.
    /// </summary>
    public const string PlayAfterFocusKey = "PlayAfterFocus";

    /// <summary>
    /// Used to identify if quick resume is enabled.
    /// </summary>
    public const string QuickResumeKey = "QuickResume";

    /// <summary>
    /// Used to store the user's active streak.
    /// </summary>
    public const string ActiveStreakKey = "ActiveStreak";

    /// <summary>
    /// Used to remember when the active streak was last updated.
    /// </summary>
    public const string ActiveStreakUpdateDateTicksKey = "ActiveStreakUpdateDate";

    /// <summary>
    /// Used to remember when the last streak reminder toast was sent.
    /// </summary>
    public const string StreakReminderLastDateTicksKey = "StreakReminderLastDateTicks";

    /// <summary>
    /// Determines if streaks are enabled by the user.
    /// </summary>
    public const string StreaksReminderEnabledKey = "StreaksReminderEnabled";

    /// <summary>
    /// Determines the last used content page.
    /// </summary>
    public const string LastUsedContentPageKey = "LastUsedContentPage";

    /// <summary>
    /// Determines the slideshow mode for an xbox device.
    /// </summary>
    public const string XboxSlideshowModeKey = "XboxSlideshowMode";

    /// <summary>
    /// Remembers if the user has seen the free trial tip.
    /// </summary>
    public const string HasViewedFreeTrialTipKey = "HasViewedFreeTrialTip";

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
        { LastUsedScreensaverKey, string.Empty },
        { HasClosedFocusHelpMessageKey, false },
        { HasClosedInterruptionMessageKey, false },
        { RecentFocusKey, Array.Empty<RecentFocusSettings>() },
        { DevicePresenceIdKey, string.Empty },
        { CompactOnFocusKey, true },
        { HasLoadedPackagedSoundsKey, false },
        { PlayAfterFocusKey, true }, 
        { QuickResumeKey, false },
        { ShowHomePageDownloadMessageKey, true },
        { ActiveStreakKey, 0 },
        { ActiveStreakUpdateDateTicksKey, 0L },
        { StreakReminderLastDateTicksKey, 0L },
        { StreaksReminderEnabledKey, false },
        { LastUsedContentPageKey, string.Empty },
        { XboxSlideshowModeKey, SlideshowMode.Video.ToString() },
        { HasViewedFreeTrialTipKey, false },
    };
}
