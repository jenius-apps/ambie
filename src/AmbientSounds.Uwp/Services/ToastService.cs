using System;
using Microsoft.Toolkit.Uwp.Notifications;

namespace AmbientSounds.Services.Uwp
{
    public class ToastService : IToastService
    {
        public void ClearScheduledToasts()
        {
            ToastNotifierCompat notifier = ToastNotificationManagerCompat.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();

            if (scheduled != null)
            {
                foreach (var toRemove in scheduled)
                {
                    notifier.RemoveFromSchedule(toRemove);
                }
            }
        }

        public void ScheduleToast(DateTime scheduleDateTime, string title, string message)
        {
            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Alarm)
                .AddText(title)
                .AddText(message)
                .Schedule(scheduleDateTime);
        }

        public void SendToast(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
    }
}
