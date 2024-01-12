using AmbientSounds.Constants;
using JeniusApps.Common.Settings.Uwp;
using JeniusApps.Common.Tools.Uwp;
using System;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;

#nullable enable

namespace AmbientSounds.Tasks;

public sealed class StreakReminderTask : IBackgroundTask
{
    public void Run(IBackgroundTaskInstance taskInstance)
    {
        var now = DateTime.Now;
        var settings = new LocalSettings(UserSettingsConstants.Defaults);

        long lastReminderDateTicks = settings.Get<long>(UserSettingsConstants.StreakReminderLastDateTicksKey);
        DateTime lastReminderDate = new(lastReminderDateTicks);

        if (lastReminderDate.Date == now.Date)
        {
            return;
        }

        // fetch last update date
        long lastUpdatedTicks = settings.Get<long>(UserSettingsConstants.ActiveStreakUpdateDateTicksKey);
        DateTime lastUpdated = new(lastUpdatedTicks);

        if (lastUpdated.Ticks >= now.Date.Ticks || now.Ticks > lastUpdated.AddHours(24).Ticks)
        {
            return;
        }

        // fetch active streak
        int activeStreak = settings.Get<int>(UserSettingsConstants.ActiveStreakKey);
        if (activeStreak <= 0)
        {
            return;
        }

        DateTime? scheduleTime = now.TimeOfDay.TotalHours < 10
            ? now.Date.AddHours(10)
            : null;

        settings.Set(UserSettingsConstants.StreakReminderLastDateTicksKey, now.Date.Ticks);

        var resourceLoader = ResourceLoader.GetForViewIndependentUse();
        new ToastService().SendToast(
            string.Format(resourceLoader.GetString("StreakReminderTitle"), activeStreak.ToString()),
            resourceLoader.GetString("StreakReminderMessage"),
            scheduledDateTime: scheduleTime,
            launchArg: LaunchConstants.StreakReminderArgument);
    }
}
