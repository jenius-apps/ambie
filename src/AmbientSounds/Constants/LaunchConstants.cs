using AmbientSounds.Services;

namespace AmbientSounds.Constants;

public class LaunchConstants
{
    public const string QuickResumeArgument = "quickResume";

    public const string StreakReminderArgument = "streakReminder";

    public const string NewSoundArgument = "newSound";

    public const string FocusSegmentArgument = "focusSegmentToast";

    public const string FocusCompleteArgument = "focusCompleteToast";

    /// <summary>
    /// Converts the given launch argument to 
    /// a corresponding content page type.
    /// </summary>
    /// <param name="toastLaunchArgument">The launch argument to convert.</param>
    /// <returns>A content page type if the argument is valid. Null, otherwise.</returns>
    public static ContentPageType? ToPageType(string toastLaunchArgument)
    {
        return toastLaunchArgument switch
        {
            NewSoundArgument => ContentPageType.Catalogue,
            FocusSegmentArgument => ContentPageType.Focus,
            FocusCompleteArgument => ContentPageType.Focus,
            _ => null
        };
    }
}
