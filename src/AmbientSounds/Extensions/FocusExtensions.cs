using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Extensions
{
    public static class FocusExtensions
    {
        public static string ToCountdownFormat(this TimeSpan remaining)
        {
            return remaining.ToString(@"mm\:ss");
        }

        public static string ToDisplayString(this SessionType sessionType, ILocalizer localizer)
        {
            return localizer.GetString($"{nameof(SessionType)}{sessionType}");
        }
    }
}
