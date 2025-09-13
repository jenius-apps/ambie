using AmbientSounds.Services;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AmbientSounds.Constants;

public class LaunchConstants
{
    public const string QuickResumeArgument = "quickResume";

    public const string StreakReminderArgument = "streakReminder";

    public const string NewSoundArgument = "newSound";

    public const string NewChannelArgument = "newChannel";

    public const string NewGuideArgument = "newGuide";

    public const string FocusSegmentArgument = "focusSegmentToast";

    public const string FocusCompleteArgument = "focusCompleteToast";

    public const string PromoCodeArgument = "promoCode";

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
            NewChannelArgument => ContentPageType.Channels,
            NewGuideArgument => ContentPageType.Meditate,
            PromoCodeArgument => ContentPageType.Settings,
            _ => null
        };
    }

    public static bool TryGetPromoCode(
        [NotNullWhen(true)] string? toastLaunchArgument,
        [NotNullWhen(true)] out string? promoCode)
    {
        if (toastLaunchArgument?.StartsWith(PromoCodeArgument, StringComparison.OrdinalIgnoreCase) is true
            && toastLaunchArgument.Split([':'], StringSplitOptions.RemoveEmptyEntries) is [string, string { Length: > 0 } code])
        {
            promoCode = code;
            return true;
        }

        promoCode = null;
        return false;
    }
}
