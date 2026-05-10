using AmbientSounds.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace AmbientSounds.Constants;

public static class LaunchConstants
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
        if (toastLaunchArgument.Contains("?")
            && toastLaunchArgument.Split('?') is [string pagePath, ..])
        {
            toastLaunchArgument = pagePath;
        }

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

    public static IReadOnlyList<string> TryGetNewIds(this string? toastLaunchArgument)
    {
        if (toastLaunchArgument is not { Length: > 0 })
        {
            return [];
        }

        if (toastLaunchArgument.Contains("?")
            && toastLaunchArgument.Split('?') is [.., string queryString])
        {
            NameValueCollection pairs = HttpUtility.ParseQueryString(queryString);

            try
            {
                if (pairs["newIds"] is string newIds)
                {
                    return newIds.Split([','], StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch { }
        }

        return [];
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
